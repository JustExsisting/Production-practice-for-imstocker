using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace JPGtoPNG
{
    class Program
    {
        static List<ImageInfo> images = new();
        static List<Point> GPSofSputnik = new();
        static void Welcom()
        {
            Console.WriteLine("Convert JPG to PNG with resolution 342x228");
            Console.WriteLine("Enter path:");
        }
        //поиск метаданных
        static string FindMeta(DirectoryInfo directory)
        {
            if (File.Exists(directory.FullName + "\\jpg.tel"))
            {
                string meta = File.ReadAllText(directory.FullName + "\\jpg.tel");
                return meta;
            }
            else
                return "jpg.tel not found";
        }
        //чтение метаданных
        static void ReadMeta(string meta)
        {
            string text = "";
            int cnt = 0;
            string name, longitude, latitude, height, pitch, roll;
            name =  longitude = latitude = height = pitch = roll = "";
            for (int i = 107; i < meta.Length; i++)
            {
                if (meta[i] != '\n' && meta[i] != '\t')
                {
                    text += meta[i];
                }
                else if (meta[i] =='\t' )
                {
                    switch (cnt)
                    {
                        case 0:
                            name = text;
                            break;
                            // case 1 - отлов ненужной UTC из метаданных, которая всегда равна 0
                        case 2:
                            longitude = text;
                            break;
                        case 3:
                            latitude = text;
                            break;
                        case 4:
                            height = text;
                            break;
                        case 5:
                            pitch = text;
                            break;
                        case 6:
                            roll = text;
                            break;
                    }
                    text = "";
                    cnt++;
                }
                else if (meta[i] == '\n')
                {
                    cnt = 0;
                    images.Add(new ImageInfo(name, longitude, latitude, height, pitch, roll, text));
                    text = "";
                }
            }
        }
   
    //создание пути для сохранения выходных данных
    static string CutLastDirectory(string directory)
        {
            string path = directory.ToString();
            for (int i = path.Length - 1; i > 0; i--)
            {
                if (path[i] == '\\')                                                    //поиск последнего спецсимвола в строке
                {
                    return path.Remove(i, (path.Length - i));
                }
            }
            return "";                                                          
        }
        //создание отдельных файлов с метаданными
        static string CreateMetaFile(string pathToSave)
        {
            //защита от случайной перезаписи уже имеющихся выходных данных
            int j = 2;
            while (true)
            {
                if (new DirectoryInfo(pathToSave).Exists)                               //если директория "**path**\input" уже есть, тогда:
                {
                    if (Char.IsDigit(pathToSave, pathToSave.Length - 1))                //проверка на наличие в конце уже ранее подставленного числа
                    {                                                                   //ПРИМЕЧАНИЕ К СТРОКЕ ВЫШЕ: работает корректно только до 19
                        pathToSave = pathToSave.Remove(pathToSave.Length - 1);
                    }
                    pathToSave += j.ToString();
                    j++;
                }
                else
                {
                    Directory.CreateDirectory(pathToSave);
                    break;
                }
            }
            for (int i = 0; i < images.Count; i++)                                      //создание нового .txt файла и заполнение данными
            {
                using (var sw = new StreamWriter(pathToSave + "\\" + images[i].Name + "_meta.txt", false, System.Text.Encoding.Default))
                {
                    sw.Write("InPitch\t{0}\nInRoll\t{1}\nInHeading\t{2}\nInHeight\t{3}\nInGpsLat\t{4}\nInGpsLon\t{5}\nSatX\t{6}\nSatY\t{7}\nSatScale\t{8}\nSatUg\t{9}",
                             images[i].Pitch,
                             images[i].Roll,
                             images[i].Heading,
                             images[i].Height,
                             images[i].Latitude,
                             images[i].Longitude,
                             images[i].SatX,
                             images[i].SatY,
                             images[i].SatScale,
                             images[i].SatUg);
                }
            }
            return pathToSave;                                                          //изменение пути для новых файлов с учетом цикла while на строке 97
        }
        static void JPGtoPNG(string path, string pathToSave)
        {
            for (int i = 0; i < images.Count; i++)
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(path + '\\' + images[i].Name + ".JPG"))
                    {
                        Size size = new Size(342, 228);
                        using (Bitmap newBitmap = new Bitmap(bitmap, size))
                        {
                            newBitmap.Save(pathToSave + '\\' + images[i].Name + "_in.PNG");
                        }
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }
        //хранение GPS координат спутникового снимка в массиве
        //GPSofSputnik[0] - ЛевВерх
        //GPSofSputnik[1] - ЛевНиз
        //GPSofSputnik[2] - ПрНиз
        //GPSofSputnik[3] - ПрВерх
        static void GPSofSputnikAnnounce()  
        {
            //левая-верхняя точка (56.77815956144973, 52.93657637802904) }---> нижняя-левая точка (56.7637818571319, 52.93657637802904)
            //правая-нижняя точка (56.7637818571319, 53.18209598837362)  }---> верхняя-правая точка   (56.77815956144973, 53.18209598837362)
            var gps = new float[4, 2];

            //gpsLat                        gpsLng
            gps[0, 0] = 56.77815956144973f;  gps[0, 1] = 52.93657637802904f; //ЛевВерх
            gps[1, 0] = 56.7637818571319f;   gps[1, 1] = 52.93657637802904f; //ЛевНиз
            gps[2, 0] = 56.7637818571319f;   gps[2, 1] = 53.18209598837362f; //ПрНиз
            gps[3, 0] = 56.77815956144973f;  gps[3, 1] = 53.18209598837362f; //ПрВерх

            for (int i = 0; i < 4; i++)
            {
                GPSofSputnik.Add(new Point(gps[i, 0], gps[i,1]));
            }
        }
        // SatXandY РАБОТАЕТ НЕПРАВИЛЬНО        
        // SatXandY РАБОТАЕТ НЕПРАВИЛЬНО
        // SatXandY РАБОТАЕТ НЕПРАВИЛЬНО
        // SatXandY РАБОТАЕТ НЕПРАВИЛЬНО
        static void SatXandY()
        {            
            float widthPx = 9630;                                                           //ширина снимка со спутника в пикселях
            float heightPx = 4377;                                                          //высота снимка со спутника в пикселях
            float width = GPSofSputnik[2].X - GPSofSputnik[1].X;                            //ширина снимка в градусах Земли
            float height = GPSofSputnik[0].Y - GPSofSputnik[1].Y;                           //высота снимка в градусах Земли
            float latitudeDividedByPixels = width / widthPx;
            float longitudeDividedByPixels = height / heightPx;
            float a = (float.Parse(images[0].Latitude, CultureInfo.InvariantCulture) * latitudeDividedByPixels);
            for (int i = 0; i < images.Count; i++)
            {
                images[i].SatX = ((float.Parse(images[i].Latitude, CultureInfo.InvariantCulture) - GPSofSputnik[0].X) * latitudeDividedByPixels).ToString();
                images[i].SatY = ((float.Parse(images[i].Longitude, CultureInfo.InvariantCulture) - GPSofSputnik[1].Y) * longitudeDividedByPixels).ToString();
            }

        }
        static void Main()
        {
            Welcom();
            string path = Console.ReadLine();
            var directory = new DirectoryInfo(path);
            string meta = FindMeta(directory);
            Stopwatch sw = new ();
            if (meta != "jpg.tel not found")
            {
                sw.Start();
                string pathToSave = CutLastDirectory(directory.ToString()) + "\\input";
                ReadMeta(meta);
                GPSofSputnikAnnounce();                
                SatXandY();
                pathToSave = CreateMetaFile(pathToSave);                
                Console.WriteLine("Path to save:\n{0}", pathToSave);
                JPGtoPNG(path, pathToSave);
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine(meta);
            }
        }
    }
}