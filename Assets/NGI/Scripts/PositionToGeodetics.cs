using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionToGeodetics : MonoBehaviour
{

    const double a = 6378137; //I think this number is for earth ellipsoid for: GPS World_Geodetic_System:_WGS_84 https://en.wikipedia.org/wiki/Earth_ellipsoid 
    const double b = 6356752.3142;
    const double f = (a - b) / a;
    const double e_sq = f * (2 - f);


    public GameObject testObject;




    //Convert unity vector position to righthanded

    //To ENU and then from ENU to ECEF

    //ECEF to Geodetic

    //Set rotation 

    //Send to Server







    public EcefPose GeodeticToEcef(double lat, double lon, double h)
    {
        double lamb, phi, s, N;
        lamb = lat * Mathf.Deg2Rad;
        phi = lon * Mathf.Deg2Rad;
        s = Math.Sin(lamb);
        N = a / Math.Sqrt(1 - e_sq * s * s);

        double sin_lambda, cos_lambda, sin_phi, cos_phi;
        sin_lambda = Math.Sin(lamb);
        cos_lambda = Math.Cos(lamb);
        sin_phi = Math.Sin(phi);
        cos_phi = Math.Cos(phi);

        double x, y, z;
        x = (h + N) * cos_lambda * cos_phi;
        y = (h + N) * cos_lambda * sin_phi;
        z = (h + (1 - e_sq) * N) * sin_lambda;

        EcefPose ep = new EcefPose();
        ep.x = x;
        ep.y = y;
        ep.z = z;
        return ep;
    }

    public Vector3 EcefToEnu(EcefPose ep, double lat_ref, double lon_ref, double h_ref)
    {
        double lamb, phi, s, N;
        lamb = lat_ref * Mathf.Deg2Rad;
        phi = lon_ref * Mathf.Deg2Rad;
        s = Math.Sin(lamb);
        N = a / Math.Sqrt(1 - e_sq * s * s);

        double sin_lambda, cos_lambda, sin_phi, cos_phi;
        sin_lambda = Math.Sin(lamb);
        cos_lambda = Math.Cos(lamb);
        sin_phi = Math.Sin(phi);
        cos_phi = Math.Cos(phi);

        double x0, y0, z0;
        x0 = (h_ref + N) * cos_lambda * cos_phi;
        y0 = (h_ref + N) * cos_lambda * sin_phi;
        z0 = (h_ref + (1 - e_sq) * N) * sin_lambda;

        Debug.Log("ep.x = " + ep.x + ", ep.y = " + ep.y + ",ep.z = " + ep.z);

        double xd, yd, zd;
        xd = ep.x - x0;
        yd = ep.y - y0;
        zd = ep.z - z0;
        Debug.Log("xd= " + xd + ", yd = " + yd + ",zd = " + zd);

        double xEast, yNorth, zUp;
        xEast = -sin_phi * xd + cos_phi * yd;
        yNorth = -cos_phi * sin_lambda * xd - sin_lambda * sin_phi * yd + cos_lambda * zd;
        zUp = cos_lambda * cos_phi * xd + cos_lambda * sin_phi * yd + sin_lambda * zd;

        //Debug.Log("xEast = "+ xEast + ",yNorth " + yNorth+ ",zUp" + zUp);

        return new Vector3((float)xEast, (float)yNorth, (float)zUp);
    }


    public class UnityPose  // class to keep pose of the camera or objects for usage in Unity with left-handed system coords
    {
        public Vector3 pos;
        public Quaternion ori;


        public UnityPose(Vector3 acpos, Quaternion acori) // convert right-handed to left-handed, by redirecting axis Y
        {
            pos = GetPosition(acpos.x, acpos.y, acpos.z);
            ori = Quaternion.Euler(-acori.eulerAngles.x, acori.eulerAngles.y, -acori.eulerAngles.z);
        }

        public static Vector3 GetPosition(float posx, float posy, float posz)
        {
            return new Vector3(posx, -posy, posz); // change the Y axis direction
        }

        public static void GetENUPose(Vector3 acpos, Quaternion acori)
        {
            Vector3 position = GetPosition(acpos.x, acpos.y, acpos.z);
            Quaternion ori = Quaternion.Euler(-acori.eulerAngles.x, acori.eulerAngles.y, -acori.eulerAngles.z);

        }

        public Vector4 GetOrientation()
        {
            return new Vector4(ori.x, ori.y, ori.z, ori.w);
        }

        public void SetCameraOriFromGeoPose(GameObject cam)
        {
            cam.transform.RotateAround(cam.transform.position, cam.transform.right, 90); // rotation around the X-axis to lift the Y-axis up
            cam.transform.RotateAround(cam.transform.position, cam.transform.up, 90); // rotation around the Y-axis (it looks up) by 90 so that the camera is on the Z-axis instead of X
        }

        public Vector4 SetObjectOriFromGeoPose()
        {
            GameObject temp = new GameObject();
            temp.transform.localRotation = this.ori;
            temp.transform.RotateAround(temp.transform.position, temp.transform.right, -90); // rotation around the X-axis to lift the Y-axis up
            Vector4 newori = new Vector4(temp.transform.localRotation.x, temp.transform.localRotation.y, temp.transform.localRotation.z, temp.transform.localRotation.w);
            Destroy(temp);
            return newori;
        }
    }

    public void SetObjectOriFromGeoPose()
    {
        //GameObject temp = new GameObject();
        //temp.transform.localRotation = this.ori;

        testObject.transform.RotateAround(testObject.transform.position, testObject.transform.right, -90);

        //testObject.transform.rotation = Quaternion.Euler(-testObject.transform.rotation.eulerAngles.x, testObject.transform.rotation.eulerAngles.y, -testObject.transform.rotation.eulerAngles.z);

        // temp.transform.RotateAround(temp.transform.position, temp.transform.right, 90); // rotation around the X-axis to lift the Y-axis up
        // Vector4 newori = new Vector4(temp.transform.localRotation.x, temp.transform.localRotation.y, temp.transform.localRotation.z, temp.transform.localRotation.w);
        // Destroy(temp);
        //  return newori;
    }




    public class EcefPose
    {
        public double x;
        public double y;
        public double z;
    }

    public class GeoPose
    {
        public double lat;
        public double lon;
        public double h;
    }


    public class ECEFtoLatLon
    {

        public GeoPose convertLocalPoseToGeoPose(Vector3 position, Quaternion quaternion)
        {
            //Check if localization exists

            /*
                    // TODO: return proper geopose, not only the global camera pose
                    console.log("WARNING: returning fake geoPose");
                    let geoPose = {
                        // TODO: fill in the geoPose properly. now simply write in our latest known global camera pose
                        "longitude": _globalImagePose.longitude,
                        "latitude": _globalImagePose.latitude,
                        "ellipsoidHeight": _globalImagePose.ellipsoidHeight,
                        "quaternion": {
                            "x": _globalImagePose.quaternion.x,
                            "y": _globalImagePose.quaternion.y,
                            "z": _globalImagePose.quaternion.z,
                            "w": _globalImagePose.quaternion.w
                        }
                    }
                    return geoPose;
            */

            GameObject localPose = new GameObject();
            localPose.transform.position = position;
            localPose.transform.rotation = quaternion;
            // localPose.updateMatrix(); //This updates the local transform
            //_ar2GeoTransformNode.addChild(localPose); //Ads new object to parent
            //_ar2GeoTransformNode.updateMatrixWorld(); //Updated all scene objects parents and children

            var localENUPose = new GameObject();
            localENUPose.transform.position = localPose.transform.position;
            //localENUPose.decompose(); breaks the matrix to pos, rot, scale vectors
            //_ar2GeoTransformNode.removeChild(localPose);

            //TODO: swap orientation axes
            //let localENUQuaternion = quat.fromValues(localENUPose.quaternion.x, localENUPose.quaternion.y, localENUPose.quaternion.z, localENUPose.quaternion.w);
            //localENUQuaternion = convertWeb2GeoQuat(localENUQuaternion);
            //localENUPose.quaternion.set(localENUQuaternion[0], localENUQuaternion[1], localENUQuaternion[2], localENUQuaternion[3]);


            var localEnuPosition = localENUPose.transform.position;
            localEnuPosition = new Vector3(localENUPose.transform.position.x, -localENUPose.transform.position.z, localENUPose.transform.position.y);

            /*
            export function convertWeb2GeoVec3(webVec3) {
                return vec3.fromValues(webVec3[0], -webVec3[2], webVec3[1]);
            }
            */
             
            double dE = localEnuPosition[0];
            double dN = localEnuPosition[1];
            double dU = localEnuPosition[2];

            double latitude = 1.0;//_globalImagePose;
            double longitude = 1.0;
            double EllipsoidHeight = 1.0;
            /*
            export function getEarthRadiusAt(latitude) {
                // https://en.wikipedia.org/wiki/Earth_ellipsoid
                // https://rechneronline.de/earth-radius/

                let lat = toRadians(latitude);
                const r1 = 6378137.0; // at Equator
                const r2 = 6356752.3142; // at poles
                let cosLat = Math.cos(lat);
                let sinLat = Math.sin(lat);

                let numerator = (r1 * r1 * cosLat) * (r1 * r1 * cosLat) + (r2 * r2 * sinLat) * (r2 * r2 * sinLat);
                let denominator = (r1 * cosLat) * (r1 * cosLat) + (r2 * sinLat) * (r2 * sinLat);

                return Math.sqrt(numerator / denominator);
            }*/



            //TODO: do proper conversion here!
            // See https://www.movable-type.co.uk/scripts/latlong.html
            //const R = 6371009; // Earth radius (assuming a sphere)
            double R = getEarthRadiusAt(latitude);
            double dLon = Mathf.Rad2Deg * (Math.Atan2(dE, R));
            double dLat = Mathf.Rad2Deg * (Math.Atan2(dN, R));
            double dHeight = dU;

            GeoPose geoPose = new GeoPose();
            geoPose.lat = latitude + dLat;
            geoPose.lon = longitude + dLon;
            geoPose.h = EllipsoidHeight + dHeight;
  
            //add rotation
      /*
                "x": localENUPose.quaternion.x,
                "y": localENUPose.quaternion.y,
                "z": localENUPose.quaternion.z,
                "w": localENUPose.quaternion.w
            */


            return geoPose;

        }





        public double getEarthRadiusAt(double latitude)
        {

            // https://en.wikipedia.org/wiki/Earth_ellipsoid
            // https://rechneronline.de/earth-radius/

            var lat = latitude * Mathf.Deg2Rad;
            const double r1 = 6378137.0; // at Equator
            const double r2 = 6356752.3142; // at poles
            double cosLat = Math.Cos(lat);
            double sinLat = Math.Sin(lat);

            double numerator = (r1 * r1 * cosLat) * (r1 * r1 * cosLat) + (r2 * r2 * sinLat) * (r2 * r2 * sinLat);
            double denominator = (r1 * cosLat) * (r1 * cosLat) + (r2 * sinLat) * (r2 * sinLat);

            return Math.Sqrt(numerator / denominator);
        }




    }








}


