using UnityEngine;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
namespace GardenSimulation.Services.Api
{
    public class APIClient
    {
        public (double, double)[] PandCoordinates;
        public (double, double)[] ParcelCoordinates;
        public IEnumerator GetCoordinatesByAddress(string address)
        {
            //steps: 
            //1: Get the centriode_rd coordinates from locationserver endpoint.
            //2: With given centriode_rd the script calculates the bbox.
            //3: Get the pand coordinates from pand url.
            //4: Get the parcel coordinates from parcel url.
            //5: Store the coordinates in the public fields, the method cant return them since IEnumerator doesnt allow other returns.
            string locationServerUrl = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/free?q={address}";
            UnityWebRequest request = UnityWebRequest.Get(locationServerUrl);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var locationServerResponse = request.downloadHandler.text;
                JObject json = JObject.Parse(locationServerResponse);
                string centriodeRd = json["response"]["docs"][0]["centroide_rd"].ToString();
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
                request = UnityWebRequest.Get(pandUrl);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string kadasterResponse = request.downloadHandler.text;
                    json = JObject.Parse(kadasterResponse);
                    JArray coordinates = (JArray)json["features"][0]["geometry"]["coordinates"][0];
                    PandCoordinates = new (double, double)[coordinates.Count];
                    for (int i = 0; i < coordinates.Count; i++)
                    {
                        JArray coordinate = (JArray)coordinates[i];
                        PandCoordinates[i] = 
                        (
                            (double)coordinate[0],
                            (double)coordinate[1]
                        );
                    }
                }
                else
                {
                    Debug.Log($"API Error: {request.error}");
                }
                string parcelUrl = $"https://api.pdok.nl/kadaster/brk-kadastrale-percelen/ogc/v1/collections/cadastralparcel/items" +
                $"?bbox={bbox[0]},{bbox[1]},{bbox[2]},{bbox[3]}" +
                $"&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&crs=http://www.opengis.net/def/crs/EPSG/0/28992" +
                $"&f=json";
                request = UnityWebRequest.Get(parcelUrl);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string kadasterResponse = request.downloadHandler.text;
                    json = JObject.Parse(kadasterResponse);
                    JArray coordinates = (JArray)json["features"][0]["geometry"]["coordinates"][0];
                    ParcelCoordinates = new (double, double)[coordinates.Count];
                    for (int i = 0; i < coordinates.Count; i++)
                    {
                        JArray coordinate = (JArray)coordinates[i];
                        ParcelCoordinates[i] = 
                        (
                            (double)coordinate[0],
                            (double)coordinate[1]
                        );
                    }
                }
                else
                {
                    Debug.Log($"API Error: {request.error}");
                }
            }
            else
            {
                Debug.Log($"API Error: {request.error}");
            }
        }
    }
}

