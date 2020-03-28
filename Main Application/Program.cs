using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Main_Application
{
    class Program
    {
        const string LAST_ACCESS_FILE = "last_access";
        const string LAST_ACCESS_DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
        static readonly System.Globalization.CultureInfo LAST_ACCESS_CULTURE_INFO = 
            System.Globalization.CultureInfo.InvariantCulture;

        // Constants that should eventually be specified by a settings file.
        const string destination_dir = "My Spotlight Backgrounds";
        const string landscape_dir = "Landscape";
        const string portrait_dir = "Portrait";
        const string square_dir = "Square";

        static void Main(string[] args)
        {
            // In future, parse a settings file and process it here.
            string landscape_destination_dir = $"{destination_dir}\\{landscape_dir}";
            string portrait_destination_dir = $"{destination_dir}\\{portrait_dir}";
            string square_destination_dir = $"{destination_dir}\\{square_dir}";

            string source_dir =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                @"\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
            
            // Read the last access file, if it exists. This will either be null (if no last access value can be
            // retrieved) or a DateTime we can use to compare with file access times.
            DateTime? last_access = null;
            try {
                string last_access_string = File.ReadAllText(LAST_ACCESS_FILE, System.Text.Encoding.UTF8);
                last_access =
                    DateTime.ParseExact(
                        last_access_string,
                        LAST_ACCESS_DATETIME_FORMAT,
                        LAST_ACCESS_CULTURE_INFO
                    );
            } catch(FileNotFoundException) {
                // Since the file wasn't found, assume this is the first run.
                // Do nothing; last_access is already null.
            } catch(IOException e) {
                // An I/O error occurred opening the file.
                // Print it and continue; let last_access remain null.
                Console.Error.WriteLine(e);
            } catch(Exception e) {
                // All other errors should crash the program.
                Console.Error.WriteLine(e);
                System.Environment.Exit(1);
            }

            // Update the last access file to the current time.
            //
            // Note: there's inherently a race condition where the environment could change during program execution.
            // We record the current time as the last access time so that, if this happens, the very worst-case
            // outcome is that we have a duplicate file. In no case will we miss a file due to this resolution of the
            // race condition.
            try {
                using (FileStream file = File.OpenWrite(LAST_ACCESS_FILE)) {
                    string datestring =
                        DateTime.UtcNow.ToString(
                            LAST_ACCESS_DATETIME_FORMAT,
                            LAST_ACCESS_CULTURE_INFO
                        );
                    file.Write(new UTF8Encoding(false).GetBytes(datestring), 0, datestring.Length);
                }
            } catch(Exception e) {
                // Report any exceptions here, but continue program execution, since we can clearly proceed.
                Console.Error.WriteLine(e);
            }

            // Process each file in the source directory.
            try {
                foreach (string file in Directory.EnumerateFiles(source_dir)) {
                    if (last_access.HasValue && File.GetLastAccessTime(file) < last_access.Value) {
                        // Our last run was after this file was created, so we should skip it.
                        continue;
                    }

                    try {
                        using (FileStream stream = File.OpenRead(file)) {
                            BitmapFrame image = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None).Frames[0];
                            if (image.PixelWidth > image.PixelHeight) {
                                // Landscape.
                                copyImage(file, landscape_destination_dir);
                            } else if (image.PixelWidth < image.PixelHeight) {
                                // Portrait.
                                copyImage(file, portrait_destination_dir);
                            } else {
                                // Square.
                                copyImage(file, square_destination_dir);
                            }
                        }
                    } catch (FileFormatException) {
                        // The file wasn't a JPEG image. No need to do anything; just skip it.
                        Console.WriteLine($"Skipping non-JPEG file: {Path.GetFileName(file)}");
                        continue;
                    } catch (Exception e) {
                        // Something went wrong with this file. Print an error and skip the file.
                        Console.Error.WriteLine($"Error processing file: {Path.GetFileName(file)}");
                        Console.Error.WriteLine(e);
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
            File.Copy(source_file_path, $"{destination_dir}\\{source_filename}.jpg");
        }
    }
}
