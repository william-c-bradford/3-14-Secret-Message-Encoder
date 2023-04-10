using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using System.Text;

class PPMMaker
{
    #region Public Properties
    public string?     PpmType         { get; private set; }
    public string?     PpmHeader       { get; private set; }
    public string[]?   PpmColorStrings { get; private set; }
    public List<byte>? PpmColorList    { get; private set; }
    public string[]?   PpmData         { get; private set; }
    public int         PpmWidth        { get; private set; }
    public int         PpmHeight       { get; private set; }
    #endregion

    #region Public Methods
    public BitmapMaker LoadPPMImage(string path)
    {
        // Open the ppm image as a FileStream
        FileStream infile = new FileStream(path, FileMode.Open);

        // Create a string builder to store file data
        StringBuilder ppmSB = new StringBuilder();

        // Loop through the loaded ppm data
        while (infile.Position < infile.Length)
        {
            // Store the bytes as chars in the string
            ppmSB.Append((char)infile.ReadByte());
        }// End while

        // Close the file stream
        infile.Close();

        // Split the string into a string array
        PpmData = ppmSB.ToString().Split('\n');

        // Set variable values
        bool inHeader   = true;
        int headerIndex = 0;
        PpmHeader       = string.Empty;

        // Read each line and add to header
        while (inHeader)
        {
            foreach (string line in PpmData)
            {
                // Add line to the header
                PpmHeader += line;

                // Add new line char
                PpmHeader += '\n';

                // Increase header index
                headerIndex++;

                // PPM type
                if (line == "P3" || line == "P6")
                {
                    PpmType = line;
                }// End if

                // PPM dimensions
                else if (!line.Contains('#') && line.Contains(' '))
                {
                    // Get image width and height
                    string[] dimensions = line.Split(' ');
                    PpmWidth  = int.Parse(dimensions[0]);
                    PpmHeight = int.Parse(dimensions[1]);
                }// End else if

                // PPM max color value
                else if (line == "255")
                {
                    // End of header
                    inHeader = false;

                    // Break out of header
                    break;
                }// End else if
            }// End foreach
        }// End while

        // Get size of image
        int imgSize = PpmWidth * PpmHeight;

        // Create new color array to store each pixel color
        Color[] colorArray = new Color[imgSize];

        // Displaying the image if type is P3
        if (PpmType == "P3")
        {
            // Skip header and add colors to string array
            PpmColorStrings = PpmData.Skip(headerIndex).ToArray();

            // Set size of color array
            colorArray = new Color[imgSize];

            // Loop through the color string array and add each r,g,b set to an index in the color array
            for (int i = 0; i < imgSize; i++)
            {
                // For every three bytes, the color index increases by one
                int byteIndex = i * 3;

                // Set the a,r,g,b values of each index of the color array
                colorArray[i].A = 255;
                colorArray[i].R = byte.Parse(PpmColorStrings[byteIndex]);
                colorArray[i].G = byte.Parse(PpmColorStrings[byteIndex + 1]);
                colorArray[i].B = byte.Parse(PpmColorStrings[byteIndex + 2]);
            }// End for
        }// End if

        // Displaying the image if type is P6
        else if (PpmType == "P6")
        {
            // Call P6 method and save the color array
            colorArray = LoadP6(path);
        }// End else if

        // Create a bitmap to hold image data
        BitmapMaker bmpImage = new BitmapMaker(PpmWidth, PpmHeight);

        // Create an index to loop through each color of the color array
        int colorIndex = 0;

        // Loop through pixel data
        for (int y = 0; y < PpmHeight; y++)
        {
            for (int x = 0; x < PpmWidth; x++)
            {
                // Set each pixel to the color data of the ppm image
                bmpImage.SetPixel(x, y, colorArray[colorIndex]);

                // Increase the index of the color array
                colorIndex++;
            }// End for
        }// End for

        // Return bitmap image of ppm file
        return bmpImage;
    }// End LoadPPMImage()

    private Color[] LoadP6(string path)
    {
        // Load the ppm file
        FileStream infile = new FileStream(path, FileMode.Open);

        // Variables
        int     imgSize = PpmWidth * PpmHeight;
        Color[] colors  = new Color[imgSize];
        PpmColorList    = null;

        // Set starting index to 0
        int index = 0;

        // Set infile position after header
        infile.Position = PpmHeader.Length;

        // Create list of bytes
        List<byte> bytesList = new List<byte>();

        // Read pixel data
        while (infile.Position < infile.Length)
        {
            // Read each byte
            colors[index].A = 255;
            colors[index].R = (byte)infile.ReadByte();
            colors[index].G = (byte)infile.ReadByte();
            colors[index].B = (byte)infile.ReadByte();

            // Add r,g,b bytes to list
            bytesList.Add(colors[index].R);
            bytesList.Add(colors[index].G);
            bytesList.Add(colors[index].B);

            // Increment index
            index++;
        }// End while

        // Close the file stream
        infile.Close();

        // Set color list to property
        PpmColorList = bytesList;

        // Return color array
        return colors;
    }// End LoadP6()
    #endregion
}// End class