using System;
using System.Globalization;

namespace JPGtoPNG
{
    class ImageInfo
    {
        public ImageInfo(string name, string longitude, string latitude, string height, string pitch, string roll, string heading)
        {
            Name = name;
            Longitude = longitude;
            Latitude = latitude;
            Height = height;
            Pitch = pitch;
            Roll = roll;
            Heading = heading;
        }
        public string Name { get; }
        public string Longitude { get; }
        public string Latitude { get; }
        public string Height { get; }
        public string Pitch { get; }
        public string Roll { get; }
        public string Heading { get; }
        public string SatX { get; set; }
        public string SatY { get; set; }
        public string SatScale { get { return (float.Parse(this.Height) / 600).ToString(); }} 
        public string SatUg { get { return (float.Parse(this.Heading, CultureInfo.InvariantCulture) / 180 * Math.PI).ToString(); } }
    }
}
