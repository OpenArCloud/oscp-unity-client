using UnityEngine; // for Vector3
using System; // for Math

public class EcefPose
{
    public double x;
    public double y;
    public double z;
};

public class GeoPosePosition {
    public double lat = 0.0;
    public double lon = 0.0;
    public double h = 0.0;
}

public class GeoPoseQuaternion {
    public double x = 0.0;
    public double y = 0.0;
    public double z = 0.0;
    public double w = 1.0;
}

// TODO: this is not GeoPose, this is just the position part of it!
public class GeoPose
{
    public double lat;
    public double lon;
    public double h;
    //GeoPosePosition position; // TODO
    //GeoPoseQuaternion quaternion;
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
        temp.transform.RotateAround(temp.transform.position, temp.transform.right, 90); // rotation around the X-axis to lift the Y-axis up
        Vector4 newori = new Vector4(temp.transform.localRotation.x, temp.transform.localRotation.y, temp.transform.localRotation.z, temp.transform.localRotation.w);
        UnityEngine.Object.Destroy(temp);
        return newori;
    }
}

public class GeoMath {
    public const double a = 6378137; //I think this number is for earth ellipsoid for: GPS World_Geodetic_System:_WGS_84 https://en.wikipedia.org/wiki/Earth_ellipsoid
    public const double b = 6356752.3142;
    public const double f = (a - b) / a;
    public const double e_sq = f * (2 - f);


    public static EcefPose GeodeticToEcef(double lat, double lon, double h)
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

    public static Vector3 EcefToEnu(EcefPose ep, double lat_ref, double lon_ref, double h_ref)
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

        //Debug.Log("ep.x = " + ep.x + ", ep.y = " + ep.y + ",ep.z = " + ep.z);

        double xd, yd, zd;
        xd = ep.x - x0;
        yd = ep.y - y0;
        zd = ep.z - z0;
        //Debug.Log("xd= " + xd + ", yd = " + yd + ",zd = " + zd);

        double xEast, yNorth, zUp;
        xEast = -sin_phi * xd + cos_phi * yd;
        yNorth = -cos_phi * sin_lambda * xd - sin_lambda * sin_phi * yd + cos_lambda * zd;
        zUp = cos_lambda * cos_phi * xd + cos_lambda * sin_phi * yd + sin_lambda * zd;

        //Debug.Log("xEast = "+ xEast + ",yNorth " + yNorth+ ",zUp" + zUp);

        return new Vector3((float)xEast, (float)yNorth, (float)zUp);
    }
} // namespace GeoMath
