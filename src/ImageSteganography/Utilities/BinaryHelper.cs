using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageSteganography.Utilities;
public static class BinaryHelper
{
    /// <summary>
    /// Convert byte to binary
    /// </summary>
    /// <param name="b"></param>
    /// <returns>01 values as string</returns>
    private static string ToBinary(this byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    /// <summary>
    /// Gets RGB color and returns binary value
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static (string red, string green, string blue) ToBinary(this Color color)
    {
        string red = color.R.ToBinary();
        string green = color.G.ToBinary();
        string blue = color.B.ToBinary();
        return (red, green, blue);
    }

    /// <summary>
    /// Convert binary to asci code
    /// </summary>
    /// <param name="binary"></param>
    /// <returns></returns>
    public static byte[] ConvertBinaryStringToByteArray(this string binary)
    {
        var list = new List<byte>();

        for (int i = 0; i < binary.Length; i += 8)
        {
            string t = binary.Substring(i, 8);

            list.Add(Convert.ToByte(t, 2));
        }

        return list.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bits"></param>
    /// <param name="bitforchange"></param>
    /// <returns></returns>
    public static string ReplaceFirstBits(this string bits, string bitforchange)
    {
        if (bitforchange.Length > 8)
        {
            throw new ArgumentOutOfRangeException("Maximum number of bits is 8");
        }

        int numberOfBits = bitforchange.Length;
        bits = bits.Substring(0, bits.Length - numberOfBits);
        bits += bitforchange;
        return bits;
    }

    /// <summary>
    /// Reads given number of bits
    /// </summary>
    /// <param name="bit"></param>
    /// <param name="numberOfBits"></param>
    /// <returns></returns>
    public static string ReadFirstBits(this string bit, byte numberOfBits)
    {
        if (numberOfBits > 8)
        {
            throw new ArgumentOutOfRangeException("Maximum number of bits is 8");
        }

        return bit.Substring(8 - numberOfBits, numberOfBits);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ConvertStringToBinary(this string text)
    {
        //store results of binary 
        string binaryresult = "";
        foreach (char item in text)
        {
            //Checkif the input is number
            if (item.ToString().Any(char.IsDigit))
            {
                //convert int to binary(we need to know if the input is number ot not cause the first 4 bits of first digits are 0000)
                binaryresult += string.Format("{0:00000000}", int.Parse(Convert.ToString(int.Parse(item.ToString()), 2)));
            }
            else
            {
                //if input is text
                binaryresult += ((byte)item).ToBinary();
            }
        }
        //fully converted
        return binaryresult;
    }

    ///Proccess binary codes to readable text
    public static string BinaryToReadableText(this string binaryText)
    {
        //the final text that decode
        string result = "";
        //the number can divide 8 bits in long string
        int index = 0;
        //the array that holds the characters bits
        string[] bits = new string[binaryText.Length / 8];

        //lets first divide the characters 
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i] = binaryText.Substring(index, 8);
            index += 8;
        }

        foreach (string item in bits)
        {
            //numbers must convert differently
            //00001010 its value of Next line
            if (item.Substring(0, 4) == "0000" && !item.Equals("00001010"))
            {
                result += Convert.ToInt32(item, 2).ToString();
            }
            else
            {
                var data = ConvertBinaryStringToByteArray(item);
                result += Encoding.ASCII.GetString(data);
            }
        }
        return result;
    }

    //Utf-32 convert string to binary
    public static string UnicodeStringToBinary(this string text)
    {
        //convert text to byte
        string binarytext =
        string.Join(
                     string.Empty,
                       Encoding.UTF32
                         .GetBytes(text)
                           .Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
        //now we have binary text
        return binarytext;
    }

    //Utf-32 convert binary to string
    public static string GetUnicodeTextFromBinary(this string binarytext)
    {
        //convert binary to asci bytes
        byte[] asciitxt = ConvertBinaryStringToByteArray(binarytext);
        return Encoding.UTF32.GetString(asciitxt);
    }

    // Convert a Bitmap object to a byte array
    public static byte[] BitmapToByteArray(this Bitmap bitmap)
    {
        // Lock the bitmap for read-only access
        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly, bitmap.PixelFormat);

        try
        {
            // Create a byte array to hold the image data
            int numBytes = bitmapData.Stride * bitmapData.Height;
            byte[] bytes = new byte[numBytes];

            // Copy the image data to the byte array
            Marshal.Copy(bitmapData.Scan0, bytes, 0, numBytes);

            return bytes;
        }
        finally
        {
            // Unlock the bitmap
            bitmap.UnlockBits(bitmapData);
        }
    }
}