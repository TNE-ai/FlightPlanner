using System;

public struct GeoLocation
{
    public double lati;
    public double longi;
    public GeoLocation(double _lati, double _longi)
    {
        lati = _lati;
        longi = _longi;
    }

    public GeoLocation(GeoLocation anchor, float x, float y)
    {
        lati = anchor.lati + (double)y / 111000;
        longi = anchor.longi + (double)x / 111000 / Math.Cos(Math.PI * anchor.lati / 180);
    }
}

