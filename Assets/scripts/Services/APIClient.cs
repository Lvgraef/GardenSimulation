using UnityEngine;
using System;
using ClipperLib;
using GardenSimulation.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
namespace GardenSimulation.Services.Api
{
    public class APIClient
    {
        public IEnumerator GetCoordinatesByAddress(string address, Action<Response> onSuccess)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                 onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "Address cant be null", Success = false});
                yield break;
            }
            string locationServerUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/free?q={address}";
            UnityWebRequest request = UnityWebRequest.Get(locationServerUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var locationServerResponse = request.downloadHandler.text;
                Debug.Log(locationServerResponse);
                JObject json = JObject.Parse(locationServerResponse);
                string? centriodeRd = null;
                string? connectedParcel = null;
               JArray docs = (JArray)json["response"]["docs"];

                foreach (JToken doc in docs)
                {
                    string? naam = doc["weergavenaam"]?.ToString();

                    if (naam == address)
                    {
                        centriodeRd = doc["centroide_rd"]?.ToString();
                        connectedParcel = doc["gekoppeld_perceel"][0].ToString();
                        break;
                    }
                }
                string[] parts = connectedParcel.Split('-');  
                string parcelNumber = parts[2];   
                string parcelSection = parts[1];
                if (string.IsNullOrWhiteSpace(centriodeRd))
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "centriodeRd was null.", Success = false});
                    yield break;
                }
                if (string.IsNullOrWhiteSpace(connectedParcel))
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "gekoppeldPerceel was null.", Success = false});
                    yield break;
                }
                int meters = 3;
                int indexOpenBracket = centriodeRd.IndexOf('(') + 1;
                int indexSpace = centriodeRd.IndexOf(' ');
                int indexClosedBracket = centriodeRd.IndexOf(')');
                double centriodeLon = Convert.ToDouble(centriodeRd.Substring(indexOpenBracket, indexSpace - indexOpenBracket));
                double centriodeLat = Convert.ToDouble(centriodeRd.Substring(indexSpace, indexClosedBracket - indexSpace));
                double[] bbox = new double[4];
                bbox[0] = centriodeLon - meters;
                bbox[1] = centriodeLat - meters;
                bbox[2] = centriodeLon + meters;
                bbox[3] = centriodeLat + meters;
                string pandUrl = $"https://api.pdok.nl/kadaster/bag/ogc/v2/collections/pand/items" +
                $"?bbox={bbox[0]},{bbox[1]},{bbox[2]},{bbox[3]}" +
                $"&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&f=json";
                string parcelUrl = $"https://api.pdok.nl/kadaster/brk-kadastrale-percelen/ogc/v1/collections/cadastralparcel/items" +
                $"?bbox={bbox[0]},{bbox[1]},{bbox[2]},{bbox[3]}" +
                $"&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&f=json";
                request = UnityWebRequest.Get(parcelUrl);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    json = JObject.Parse(response);
                    var features = json["features"] as JArray;
                    JArray? parcelCoordinates = null;
                    Debug.Log(parcelSection + parcelNumber);
                    foreach (var feature in features)
                    {
                        string refId = feature["properties"]?["national_cadastral_reference"]?.ToString();
                        Debug.Log(refId);
                        if (refId.Contains(parcelSection + parcelNumber))
                        {
                            parcelCoordinates = feature["geometry"]?["coordinates"]?[0] as JArray;
                            break;
                        }
                    }
                    if (parcelCoordinates == null)
                    {
                        onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "No match found with connected parcel", Success = false});
                        yield break;
                    }
                    request = UnityWebRequest.Get(pandUrl);
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        response = request.downloadHandler.text;
                        json = JObject.Parse(response);
                        features = json["features"] as JArray;
                        JArray? pandCoordinates = null;
                        List<Vector2> parcelPoly = ConvertToPolygon(parcelCoordinates);
                        double biggestOverlap = 0;
                        foreach (var feature in features)
                        {
                            var coords = feature["geometry"]?["coordinates"]?[0] as JArray;
                            if (coords == null) continue;

                            List<Vector2> pandPoly = ConvertToPolygon(coords);

                            double overlap = ComputePolygonIntersectionArea(parcelPoly, pandPoly);

                            if (overlap > biggestOverlap)
                            {
                                biggestOverlap = overlap;
                                pandCoordinates = coords;
                            }
                        }
                        if (pandCoordinates == null)
                        {
                            onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "pandCoordinates were null.", Success = false});
                            yield break;
                        }
                        onSuccess(new Response{ParcelCoordinates = ConvertToCoordinateArray(parcelCoordinates), PandCoordinates = ConvertToCoordinateArray(pandCoordinates), Message = "", Success = true});
                        yield break;
                    }
                    else
                    {
                        onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = $"API Error: {request.error}", Success = false});
                    }
                }
                else
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = $"API Error: {request.error}", Success = false});
                }
            }
            else
            {
                onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = $"API Error: {request.error}", Success = false});
            }
        }
        private (double, double)[] ConvertToCoordinateArray(JArray coordinates)
        {
            (double, double)[] _coordinates = new (double, double)[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
            {
                JArray coordinate = (JArray)coordinates[i];
                _coordinates[i] = 
                (
                    (double)coordinate[0],
                    (double)coordinate[1]
                );
            }
            return _coordinates;
        }
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
        private List<IntPoint> ToClipper(List<Vector2> poly)
        {
            const double scale = 1000.0; // enough precision for RD coords
            List<IntPoint> result = new List<IntPoint>();

            foreach (var p in poly)
                result.Add(new IntPoint(p.x * scale, p.y * scale));

            return result;
        }
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


