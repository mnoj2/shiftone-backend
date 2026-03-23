using Microsoft.Extensions.Configuration;
using ShiftOne.Application.Interfaces;

namespace ShiftOne.Application.Services;

public class LocationService : ILocationService {

    private readonly double _latitude;
    private readonly double _longitude;
    private readonly double _radiusKm;

    public LocationService(IConfiguration config) {
        _latitude = config.GetValue<double>("CompanyLocation:Latitude");
        _longitude = config.GetValue<double>("CompanyLocation:Longitude");
        _radiusKm = config.GetValue<double>("CompanyLocation:RadiusKm");
    }

    public void IsWithinRadius(double userLat, double userLng) {
        const double R = 6371;

        var dLat = ToRad(_latitude - userLat);
        var dLng = ToRad(_longitude - userLng);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(userLat)) * Math.Cos(ToRad(_latitude)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;

        if (distance > _radiusKm) {
            throw new Exception($"Action failed. You're outside the allowed location");
        }
    }

    private double ToRad(double deg) => deg * Math.PI / 180;
}