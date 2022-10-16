using System.Collections.Generic;
using UnityEngine;

namespace NGI.Api
{
    //SCR = spatial content record
    public class SCRItem
    {
        public string type;    
        public Content content;

        // TODO: why are these NonSerialized?
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
        [System.NonSerialized]
        public bool isToFarAway; // TODO: fix typo
        [System.NonSerialized]
        public bool isAssetBundle;
    }

    public class Position
    {
        public double lat;
        public double lon;
        public double h;
    }

    // TODO: rename to GeoPose (make sure it does nor collide with AC's geopose definition)
    public class GeoPosition
    {
        public Position position;
        public Dictionary<string, float> quaternion; // TODO: introduce Quaternion class here
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
