using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public class APIClient
    {
        /// <summary>
        /// Get the coordinates of a parcel and a pand based on an address using the PDOK API.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public IEnumerator GetCoordinatesByAddress(string address, Action<Response> onSuccess)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                fail(onSuccess, "Address cant be null.");
                yield break;
            }

            // location data

            string locationServerUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/free?q={address}";

            JObject? json = null;

            yield return GetJson(
                locationServerUrl,
                (result) => { json = result; },
                (error) => { fail(onSuccess, error); });

            if (json == null) yield break;

            if (!TryParseLocation(json, address, out var centroidRd, out var parceRef))
            {
                fail(onSuccess, "API Error: could not parse location data");
                yield break;
            }

            if (!TryParseParcelRef(parceRef, out var section, out var number))
            {
                fail(onSuccess, "API Error: invalid parcel reference");
                yield break;
            }

            //URLs

            var bbox = BuildBbox(centroidRd, 3);

            string parceUrl = BuildParcelUrl(bbox);
            string pandUrl = BuildPandUrl(bbox);

            //parcel data

            JObject parcelJson = null;

            yield return GetJson(
                parceUrl,
                (result) => { parcelJson = result; },
                (error) => { fail(onSuccess, $"API Error: {error}"); });

            if (parcelJson == null) yield break;

            if (!TryGetParcel(parcelJson, section, number, out var parcelCoordinates))
            {
                fail(onSuccess, "No match found with connected parcel");
                yield break;
            }

            //pand data

            JObject pandJson = null;

            yield return GetJson(
                pandUrl,
                (result) => { pandJson = result; },
                (error) => { fail(onSuccess, $"API Error: {error}"); });

            if (pandJson == null) yield break;

            if (!TryGetBestPand(pandJson, parcelCoordinates, out var pandCoordinates))
            {
                fail(onSuccess, "pandCoordinates were null");
                yield break;
            }

            onSuccess(new Response
            {
                ParcelCoordinates = ConvertToCoordinateArray(parcelCoordinates),
                PandCoordinates = ConvertToCoordinateArray(pandCoordinates),
                Message = "",
                Success = true
            });
        }

        /// <summary>
        /// request helper method to get json from url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onSucces"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private IEnumerator GetJson(string url, Action<JObject> onSucces, Action<string> onError) { 
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) { 
                onError?.Invoke(request.error);
                yield break;
            }
            onSucces?.Invoke(JObject.Parse(request.downloadHandler.text));
        }

        /// <summary>
        /// fail helper method to return a failed response.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="message"></param>
        private void fail(Action<Response> callback, string message) {
            callback?.Invoke(new Response { 
                ParcelCoordinates = null,
                PandCoordinates = null,
                Message = message,
                Success = false
            });
        }

        /// <summary>
        /// parse the location data from the json response and return centroidRd and parcelRef if found.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="adress"></param>
        /// <param name="centroidRd"></param>
        /// <param name="parcelRef"></param>
        /// <returns></returns>
        private bool TryParseLocation(JObject json, string adress, out string centroidRd, out string parcelRef) {
            centroidRd = null;
            parcelRef = null;

            JArray docs = (JArray)json["response"]?["docs"];
            if (docs is null) return false;

            foreach (var doc in docs) {
                string naam = doc["weergavenaam"]?.ToString();

                if (naam == adress) { 
                    centroidRd= doc["centroide_rd"]?.ToString();
                    parcelRef = doc["gekoppeld_perceel"]?[0]?.ToString();
                    break;
                }

            }
            return !string.IsNullOrWhiteSpace(centroidRd) && !string.IsNullOrWhiteSpace(parcelRef);
        }

        /// <summary>
        /// parse the parcel reference and return section and number if found.
        /// </summary>
        /// <param name="parcelRef"></param>
        /// <param name="section"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private bool TryParseParcelRef(string parcelRef, out string section, out string number)
        {
            section = null;
            number = null;

            var parts = parcelRef?.Split('-');
            if (parts == null || parts.Length < 3) return false;

            section = parts[1];
            number = parts[2];

            return true;
        }

        /// <summary>
        /// parse the parcel json and return the coordinates if found.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="section"></param>
        /// <param name="number"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        private bool TryGetParcel(JObject json, string section, string number, out JArray coords)
        {
            coords = null;

            var features = json["features"] as JArray;
            if (features == null) return false;

            foreach (var feature in features)
            {
                string refId = feature["properties"]?["national_cadastral_reference"]?.ToString();
                if (refId == null) continue;

                if (refId.Contains(section + number))
                {
                    coords = feature["geometry"]?["coordinates"]?[0] as JArray;
                    break;
                }
            }

            return coords != null;
        }

        /// <summary>
        /// get the best matching pand coordinates from the json response based on the parcel coordinates.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="parcelCoords"></param>
        /// <param name="bestCoords"></param>
        /// <returns></returns>
        private bool TryGetBestPand(JObject json, JArray parcelCoords, out JArray bestCoords)
        {
            bestCoords = null;

            var features = json["features"] as JArray;
            if (features == null) return false;

            var parcelPoly = ConvertToPolygon(parcelCoords);

            double bestOverlap = 0;

            foreach (var feature in features)
            {
                var coords = feature["geometry"]?["coordinates"]?[0] as JArray;
                if (coords == null) continue;

                var pandPoly = ConvertToPolygon(coords);

                double overlap = ComputePolygonIntersectionArea(parcelPoly, pandPoly);

                if (overlap > bestOverlap)
                {
                    bestOverlap = overlap;
                    bestCoords = coords;
                }
            }

            return bestCoords != null;
        }

        /// <summary>
        /// build a bounding box around the centroid coordinates with a given distance in meters.
        /// </summary>
        /// <param name="centroidRd"></param>
        /// <param name="meters"></param>
        /// <returns></returns>
        private (double, double, double, double) BuildBbox(string centroidRd, int meters)
        {
            int open = centroidRd.IndexOf('(') + 1;
            int space = centroidRd.IndexOf(' ');
            int close = centroidRd.IndexOf(')');

            double x = Convert.ToDouble(centroidRd.Substring(open, space - open));
            double y = Convert.ToDouble(centroidRd.Substring(space, close - space));

            return (x - meters, y - meters, x + meters, y + meters);
        }

        /// <summary>
        /// build the url for the parcel api with the given bounding box.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private string BuildParcelUrl((double minX, double minY, double maxX, double maxY) b)
        {
            return
                $"https://api.pdok.nl/kadaster/brk-kadastrale-percelen/ogc/v1/collections/cadastralparcel/items" +
                $"?bbox={b.minX},{b.minY},{b.maxX},{b.maxY}" +
                $"&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992&f=json";
        }

        /// <summary>
        /// build the url for the pand api with the given bounding box.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private string BuildPandUrl((double minX, double minY, double maxX, double maxY) b)
        {
            return
                $"https://api.pdok.nl/kadaster/bag/ogc/v2/collections/pand/items" +
                $"?bbox={b.minX},{b.minY},{b.maxX},{b.maxY}" +
                $"&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992&f=json";
        }

        /// <summary>
        /// convert the coordinates from a JArray to an array of tuples of doubles.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        private (double, double)[] ConvertToCoordinateArray(JArray coordinates)
        {
            (double, double)[] coordinateArray = new (double, double)[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
            {
                JArray coordinate = (JArray)coordinates[i];
                coordinateArray[i] =
                (
                    (double)coordinate[0],
                    (double)coordinate[1]
                );
            }

            return coordinateArray;
        }

        /// <summary>
        /// convert the coordinates from a JArray to a list of Vector2.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        private List<Vector2> ConvertToPolygon(JArray coords)
        {
            List<Vector2> poly = new List<Vector2>();
            foreach (var c in coords)
            {
                double x = (double)c[0];
                double y = (double)c[1];
                poly.Add(new Vector2((float)x, (float)y));
            }

            return poly;
        }

        /// <summary>
        /// convert a list of Vector2 to a list of IntPoint for use with the Clipper library.
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        private List<IntPoint> ToClipper(List<Vector2> poly)
        {
            const double scale = 1000.0; // enough precision for RD coords
            List<IntPoint> result = new List<IntPoint>();

            foreach (var p in poly)
                result.Add(new IntPoint(p.x * scale, p.y * scale));

            return result;
        }

        /// <summary>
        /// compute the intersection area of two polygons represented as lists of Vector2 using the Clipper library.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double ComputePolygonIntersectionArea(List<Vector2> a, List<Vector2> b)
        {
            const double scale = 1000.0;

            List<IntPoint> subj = ToClipper(a);
            List<IntPoint> clip = ToClipper(b);

            Clipper c = new Clipper();
            c.AddPath(subj, PolyType.ptSubject, true);
            c.AddPath(clip, PolyType.ptClip, true);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            double area = 0;
            foreach (var poly in solution)
                area += Clipper.Area(poly);

            return Math.Abs(area) / (scale * scale);
        }
    }
}