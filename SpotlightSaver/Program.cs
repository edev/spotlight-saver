using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SpotlightSaver
{
    class Program
    {
        // Constants that should eventually be specified by a settings file.
        const string destination_dir = "My Spotlight Backgrounds";
        const string landscape_dir = "Landscape";
        const bool include_landscape = true;
        const string portrait_dir = "Portrait";
        const bool include_portrait = false;
        const string square_dir = "Square";
        const bool include_square = true;
        const uint min_width = 1080;
        const uint min_height = 1080;

        static void Main(string[] args)
        {
            // In future, parse a settings file and process it here.
            string landscape_destination_dir = $"{destination_dir}\\{landscape_dir}";
            string portrait_destination_dir = $"{destination_dir}\\{portrait_dir}";
            string square_destination_dir = $"{destination_dir}\\{square_dir}";

            string source_dir =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                @"\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
            
            // Process each file in the source directory.
            try {
                foreach (string file in Directory.EnumerateFiles(source_dir)) {
                    string filename = Path.GetFileName(file);

                    try {
                        using (FileStream stream = File.OpenRead(file)) {
                            BitmapFrame image = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None).Frames[0];

                            if (image.PixelWidth < min_width || image.PixelHeight < min_height) {
                                // The image is smaller than the minimum dimensions. Skip it.
                                Console.WriteLine($"Skipping too-small image: {filename}");
                                continue;
                            }

                            if (image.PixelWidth > image.PixelHeight) {
                                // Landscape.
                                if (include_landscape) {
                                    copyImage(file, landscape_destination_dir);
                                } else {
                                    Console.WriteLine($"Excluding landscape image: {filename}");
                                }
                            } else if (image.PixelWidth < image.PixelHeight) {
                                // Portrait.
                                if (include_portrait) {
                                    copyImage(file, portrait_destination_dir);
                                } else {
                                    Console.WriteLine($"Excluding portrait image: {filename}");
                                }
                            } else if (include_square) {
                                // Square.
                                copyImage(file, square_destination_dir);
                                } else {
                                    Console.WriteLine($"Excluding square image: {filename}");
                            }
                        }
                    } catch (FileFormatException) {
                        // The file wasn't a JPEG image. No need to do anything; just skip it.
                        Console.WriteLine($"Skipping non-JPEG file: {filename}");
                        continue;
                    } catch (Exception e) {
                        if (e.Message.IndexOf("exists") >= 0) {
                            // The file exists, so we'll just skip it. This isn't really an error;
                            // it's normal, expected behavior.
                            Console.WriteLine($"File already exists: {filename}.jpg");
                        } else {
                            // Something went wrong with this file. Print an error and skip the file.
                            Console.Error.WriteLine($"Error processing file: {filename}");
                            Console.Error.WriteLine(e.Message);
                        }
                        continue;
                    }
                }
            } catch(Exception e) {
                // We need to log the error, but we can continue running. It's possible a file might be inaccessable
                // for some reason, in which case it will be skipped. Unfortunately, it will be skipped forever due to
                // the last access mechanic, but short of maintaining some sort of database, that's unavoidable, and
                // for such a simple application, a database is overkill.
                Console.Error.WriteLine(e);
            }
        }

        private static void copyImage(string source_file_path, string destination_dir) {
            Directory.CreateDirectory(destination_dir);
            string source_filename = Path.GetFileName(source_file_path);
            Console.WriteLine($"Copying image: {source_filename}.jpg");
            File.Copy(source_file_path, $"{destination_dir}\\{source_filename}.jpg");
        }
    }
}
