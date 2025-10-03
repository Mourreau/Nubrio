namespace Nubrio.Domain.Models;

public record Coordinates
{
    public Coordinates(double latitude, double longitude)
    {
        if (!ValidateRange(-90, 90, latitude) // Широта должна быть в диапазоне от -90 до 90 градусов
            || double.IsNaN(latitude) 
            || double.IsInfinity(latitude)) 
        {
            throw new ArgumentOutOfRangeException(
                nameof(Latitude), 
                latitude, 
                "Latitude must be between -90 and 90.");
        }
        
        if (!ValidateRange(-180, 180, longitude) // Долгота должна быть в диапазоне от -180 до 180 градусов
            || double.IsNaN(longitude) 
            || double.IsInfinity(longitude)) 
        {
            throw new ArgumentOutOfRangeException(
                nameof(Longitude), 
                longitude, 
                "Longitude must be between -180 and 180.");
        } 
        
        Latitude = latitude;
        
        Longitude = longitude;
    }
    
    public double Latitude { get; } // Широта
    public double Longitude { get; } // Долгота
    
    

    private static bool ValidateRange(double min, double max, double value)
    {
        return value >= min && value <= max;
    }
}