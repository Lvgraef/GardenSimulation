using UnityEngine;
using System;
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
            string locationServerUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/free?q={address}";
            UnityWebRequest request = UnityWebRequest.Get(locationServerUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var locationServerResponse = request.downloadHandler.text;
                JObject json = JObject.Parse(locationServerResponse);
                string? centriodeRd = json["response"]?["docs"]?[0]?["centroide_rd"]?.ToString();
                if (string.IsNullOrWhiteSpace(centriodeRd))
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = "centriodeRd was null.", Success = false});
                    yield break;
                }
                int meters = 2;
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
                CoordinateResponse pandCoordinateResponse = null;
                CoordinateResponse parcelCoordinateResponse = null;
                yield return GetVerticesCoordinates(pandUrl, coordinatesResponse =>
                {
                    pandCoordinateResponse = coordinatesResponse;
                });
                yield return GetVerticesCoordinates(parcelUrl, coordinatesResponse =>
                {
                    parcelCoordinateResponse = coordinatesResponse;
                });
                if (!pandCoordinateResponse.Success)
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = pandCoordinateResponse.Message, Success = false});
                    yield break;
                }
                else if (!parcelCoordinateResponse.Success)
                {
                    onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = parcelCoordinateResponse.Message, Success = false});
                    yield break;
                }
                onSuccess(new Response{ParcelCoordinates = parcelCoordinateResponse.Coordinates, PandCoordinates = pandCoordinateResponse.Coordinates, Message = "", Success = true});
            }
            else
            {
                onSuccess(new Response{ParcelCoordinates = null, PandCoordinates = null, Message = $"API Error: {request.error}", Success = false});
            }
        }
    
        public IEnumerator GetVerticesCoordinates(string url, Action<CoordinateResponse> onSuccess)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                JObject json = JObject.Parse(response);
                JArray? coordinates =
                    json["features"]?[0]?["geometry"]?["coordinates"]?[0] as JArray;
                if (coordinates == null)
                {
                    onSuccess(new CoordinateResponse{Success = false, Message = "coordinates were null", Coordinates = null});
                    yield break;
                }
                (double, double)[] _coordinates = new (double, double)[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                {
                    JArray coordinate = (JArray)coordinates[i];
                    if (coordinate == null || coordinate.Count < 2)
                    {
                        onSuccess(new CoordinateResponse{Success = false, Message = "Some coordinates were missing.", Coordinates = null});
                        yield break;
                    }
                    _coordinates[i] = 
                    (
                        (double)coordinate[0],
                        (double)coordinate[1]
                    );
                }
                onSuccess(new CoordinateResponse{Success = true, Message = "", Coordinates = _coordinates});
            }
            else
            {
                onSuccess(new CoordinateResponse{Success = false, Message = $"API Error: {request.error}", Coordinates = null});
            }
        }
    }
}


