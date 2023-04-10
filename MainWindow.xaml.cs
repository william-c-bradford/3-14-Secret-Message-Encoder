using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace _3_14_Secret_Message_Encoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global Variables
        static string? loadedImage          = "";
        PPMMaker       ppmImage             = new PPMMaker();
        List<byte>     encodedColorList     = new List<byte>();
        string[]?      encodedColorStrings;
        #endregion

        #region Public Methods
        public MainWindow()
        {
            InitializeComponent();
        }// End MainWindow()
        #endregion

        #region Private Methods
        private void MuiOpen_Click(object sender, RoutedEventArgs e)
        {
            // Create open file dialog
            OpenFileDialog openFile = new OpenFileDialog();

            // Setup parameters for open file dialog
            openFile.DefaultExt = ".ppm";
            openFile.Filter     = "PPM Files (.ppm)|*.ppm";
            openFile.Title      = "Open PPM File";

            // Process dialog results to determine if a file was opened
            if (openFile.ShowDialog() == true)
            {
                // Store file path
                loadedImage = openFile.FileName;

                // Call LoadImage method
                LoadImage(loadedImage);

                // Store the file info of loaded image
                FileInfo fileInfo = new FileInfo(loadedImage);

                // Set the image file name box
                txtOriginalFilename.Content = fileInfo.Name;

                // Clear out encoded image box and encoded save name
                imgEncodedMain.Source = null;
                txtEncodedFilename.Content = null;

                // Collapse the save menu
                muiSave.Visibility = Visibility.Collapsed;

                // Clear the error text block
                txtError.Text = null;
            }// End if
        }// End MuiOpen_Click()

        private void MuiSave_Click(object sender, RoutedEventArgs e)
        {
            // Store the file info of loaded image
            FileInfo fs = new FileInfo(loadedImage);

            // Create a new save file dialog
            SaveFileDialog saveFile = new SaveFileDialog();

            // Set the name to the loaded image file name
            saveFile.FileName       = fs.Name;

            // Setup parameters for save file dialog
            saveFile.DefaultExt = ".ppm";
            saveFile.Filter     = "PPM Files (.ppm)|*.ppm";
            saveFile.Title      = "Save PPM File";

            if (saveFile.ShowDialog() == true)
            {
                // Open the FileStream
                FileStream outFile = new FileStream(saveFile.FileName, FileMode.Create);

                // If ppm type is P3
                if (ppmImage.PpmType == "P3")
                {
                    string colors = string.Join("\n", encodedColorStrings);
                    // Store old header data and new encoded color data in string
                    string encodedString = ppmImage.PpmHeader + colors;
                    
                    // Loop through each char in string
                    foreach (char c in encodedString)
                    {
                        // Write each char to the file as a byte
                        outFile.WriteByte((byte)c);
                    }// End foreach
                }// End if

                // Else if ppm type is P6
                else if (ppmImage.PpmType == "P6")
                {
                    // Loop through each char in header
                    foreach (char c in ppmImage.PpmHeader)
                    {
                        // Write each char to the file as a byte
                        outFile.WriteByte((byte)c);
                    }// End foreach

                    foreach (byte b in encodedColorList)
                    {
                        // Write each byte of the color to file
                        outFile.WriteByte(b);
                    }// End foreach
                }// End else if

                // Close the FileStream
                outFile.Close();

                // Get file info for new file
                FileInfo newInfo = new FileInfo(saveFile.FileName);

                // Display the name of the saved file
                txtEncodedFilename.Content = newInfo.Name;
            }// End if
        }// End MuiSave_Click()

        private void BtnEncode_Click(object sender, RoutedEventArgs e)
        {
            // Clear error box
            txtError.Text = string.Empty;

            // Clear color lists
            encodedColorStrings = null;
            encodedColorList.Clear();

            // Message is not blank and image is loaded
            if (txtMessage.Text.Length > 0 && loadedImage != "")
            {
                // If message fits image pixel size
                if (txtMessage.Text.Length <= ppmImage.PpmWidth * ppmImage.PpmHeight)
                {
                    // Get text from the message box
                    string message = txtMessage.Text;

                    // Assign the chars to a char array
                    char[] messageChars = message.ToCharArray();

                    // Create new bitmap image
                    BitmapMaker bmpImage = ppmImage.LoadPPMImage(loadedImage);

                    // Copy P3 color list
                    if (ppmImage.PpmType == "P3")
                    {
                        // Make a copy of the ppm color string
                        encodedColorStrings = ppmImage.PpmColorStrings.ToArray();
                    }// End if

                    // Copy P6 color list
                    else if (ppmImage.PpmType == "P6")
                    {
                        // Loop through original ppm colors
                        foreach (byte b in ppmImage.PpmColorList)
                        {
                            // Make a copy of the ppm color list
                            encodedColorList.Add(b);
                        }// End foreach
                    }// End if

                    // Index to loop through message chars
                    int charIndex  = 0;

                    // Index to change encoded ppm color data
                    int encIndex   = 0;

                    // Create new color data
                    Color newColor = new Color();

                    // Encoding finished
                    bool encodingFinished = false;

                    // Loop through the bitmap image
                    for (int y = 0; y < bmpImage.Height && !encodingFinished; y++)
                    {
                        for (int x = 0; x < bmpImage.Width && !encodingFinished; x++)
                        {
                            // Check if message has been encoded
                            bool messageEncoded = charIndex == messageChars.Length;

                            // Get the pixel data
                            byte[] pixelData = bmpImage.GetPixelData(x, y);

                            if (charIndex < messageChars.Length)
                            {
                                // Separate message char digits
                                int messageNum = messageChars[charIndex];

                                // Round pixel data values
                                int rounded0 = (pixelData[0] + 5) / 10 * 10;
                                    if (rounded0 >= 250) rounded0 = 240;
                                int rounded1 = (pixelData[1] + 5) / 10 * 10;
                                    if (rounded1 >= 250) rounded1 = 250;
                                int rounded2 = (pixelData[2] + 5) / 10 * 10;
                                    if (rounded2 >= 250) rounded2 = 240;

                                // Extract ones
                                int ones = messageNum % 10;
                                messageNum /= 10;

                                // Extract tens
                                int tens = messageNum % 10;
                                messageNum /= 10;

                                // Extract hundreds
                                int hundreds = messageNum % 10;

                                // Set value of new colors to value of message char
                                newColor.A = 255;
                                newColor.R = (byte)(rounded0 + tens);
                                newColor.G = (byte)(rounded1 + hundreds);
                                newColor.B = (byte)(rounded2 + ones);

                                // Set new color value to pixel
                                bmpImage.SetPixel(x, y, newColor);

                                // If ppm type is P3
                                if (ppmImage.PpmType == "P3")
                                {
                                    // Change the value of the new ppm
                                    encodedColorStrings[encIndex]     = Convert.ToString(newColor.R);
                                    encodedColorStrings[encIndex + 1] = Convert.ToString(newColor.G);
                                    encodedColorStrings[encIndex + 2] = Convert.ToString(newColor.B);
                                }// End if

                                // Else if ppm type is P6
                                else if (ppmImage.PpmType == "P6")
                                {
                                    // Change the value of the new ppm
                                    encodedColorList[encIndex]     = newColor.R;
                                    encodedColorList[encIndex + 1] = newColor.G;
                                    encodedColorList[encIndex + 2] = newColor.B;
                                }// End else if
                                
                                // Increase char index
                                charIndex ++;

                                // Increase encIndex
                                encIndex += 3;
                            }// End if

                            // If message has been encoded and message was less than length of image size
                            if (messageEncoded && messageChars.Length < (ppmImage.PpmWidth * ppmImage.PpmHeight))
                            {
                                // Round pixel data values
                                int rounded0 = (pixelData[0] + 5) / 10 * 10; if (rounded0 >= 250) rounded0 = 250;
                                int rounded1 = (pixelData[1] + 5) / 10 * 10; if (rounded1 >= 250) rounded1 = 250;
                                int rounded2 = (pixelData[2] + 5) / 10 * 10; if (rounded2 >= 250) rounded2 = 250;

                                // Set value of new colors to value of message char
                                newColor.A = 255;
                                newColor.R = (byte)rounded0;
                                newColor.G = (byte)rounded1;
                                newColor.B = (byte)rounded2;

                                // Set new color value to pixel
                                bmpImage.SetPixel(x, y, newColor);

                                if (ppmImage.PpmType == "P3")
                                {
                                    // Change the value of the new ppm
                                    encodedColorStrings[encIndex]     = Convert.ToString(newColor.R);
                                    encodedColorStrings[encIndex + 1] = Convert.ToString(newColor.G);
                                    encodedColorStrings[encIndex + 2] = Convert.ToString(newColor.B);
                                }// End if

                                else if (ppmImage.PpmType == "P6")
                                {
                                    // Change the value of the new ppm
                                    encodedColorList[encIndex]     = newColor.R;
                                    encodedColorList[encIndex + 1] = newColor.G;
                                    encodedColorList[encIndex + 2] = newColor.B;
                                }// End else if

                                // Stop loops
                                encodingFinished = true;
                            }// End if
                        }// End for
                    }// End for

                    // Make bitmap image writeable
                    WriteableBitmap wbmImage = bmpImage.MakeBitmap();

                    // Display encoded bitmap image
                    imgEncodedMain.Source = wbmImage;

                    // Make save menu visible
                    muiSave.Visibility = Visibility.Visible;
                }// End if

                // Else if message does not fit image pixel size
                else if (txtMessage.Text.Length > ppmImage.PpmWidth * ppmImage.PpmHeight)
                {
                    txtError.Text = "\tERROR!!!\nENCODED MESSAGE IS TOO LONG FOR LOADED IMAGE";
                }// End else if
            }// End if

            // Message is blank and image is loaded
            if (txtMessage.Text.Length == 0 && loadedImage != "")
            {
                txtError.Text = "\tERROR!!!\nENTER A MESSAGE TO ENCODE";
            }// End if

            // Message is not blank and image is not loaded
            if (txtMessage.Text.Length > 0 && loadedImage == "")
            {
                txtError.Text = "\tERROR!!!\nOPEN A PPM IMAGE FROM THE FILE MENU";
            }// End if

            // Message is blank and image is not loaded
            if (txtMessage.Text.Length == 0 && loadedImage == "")
            {
                txtError.Text = "\tERROR!!!\nOPEN A PPM IMAGE FROM THE FILE MENU AND ENTER A MESSAGE TO ENCODE";
            }// End if
        }// End BtnEncode_Click

        private void LoadImage(string path)
        {
            // Create a bitmap image to save the loaded image
            BitmapMaker bmpImage = ppmImage.LoadPPMImage(path);

            // Create a new bitmap of the original image
            WriteableBitmap wbmImage = bmpImage.MakeBitmap();

            // Set image control to display the original image
            imgOriginalMain.Source = wbmImage;
        }// End LoadImage()

        private void TxtEntered(object sender, TextChangedEventArgs e)
        {
            // Variable to store characters remaining
            int charsRemaining = 256 - txtMessage.Text.Length;

            // Change text to characters remaining
            txtCountCharsRemaining.Text = charsRemaining.ToString();
        }// End TxtEntered()
        #endregion
    }// End class MainWindow
}// End namespace