using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGI.Api
{

    //Spatial-service-record = SSR
    public class SpatialServiceRecord
    {
        public string type;    
        public Content content;


        [System.NonSerialized]
        public string id;
        [System.NonSerialized]
        public float timestamp;
        [System.NonSerialized]
        public string tenant;
        [System.NonSerialized]
        public Vector3 Position;
        [System.NonSerialized]
        public Vector4 Orientation;

    }

    public class GeoPosition
    {
        public double latitude;
        public double longitude;
        public double ellipsoidHeight;
  
        public Dictionary<string, float> quaternion;
    }

    public class Content
    {
        public string bbox;
        public string description;
        public string id;
        public string placeKey;
        public string type;
        public string title;

        public float size;
        
        public GeoPosition geopose;

        public IList<Dictionary<string, string>> definitions;
        public IList<Dictionary<string, string>> refs;
        public IList<string> keywords;

    }



}