namespace Nubrio.Domain.Models;

public record Coordinates
{
    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    
    private double _latitude; // Широта
    private double _longitude; // Долгота
    
    public double Latitude 
    { 
        get => _latitude;
        init
        {
            if (!ValidateRange(-90, 90, value) // Широта должна быть в диапазоне от -90 до 90 градусов
                || double.IsNaN(value) 
                || double.IsInfinity(value)) 
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Latitude), 
                    value, 
                    "Latitude must be between -90 and 90.");
            }

            _latitude =  value;
        }
    }

    public double Longitude
    {
        get => _longitude;
        init
        {
            if (!ValidateRange(-180, 180, value) // Долгота должна быть в диапазоне от -180 до 180 градусов
                || double.IsNaN(value) 
                || double.IsInfinity(value)) 
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Longitude), 
                    value, 
                    "Longitude must be between -180 and 180.");
            } 
            
            _longitude = value;
        }
    }

    private bool ValidateRange(double min, double max, double value)
    {
        return value >= min && value <= max;
    }
}