/*
 *      SharePodLib - A library for interacting with an iPod.
 *      Jeffrey Harris 2006-2010
 *      Website: http://www.getsharepod.com/fordevelopers
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using SharePodLib.Exceptions;

namespace SharePodLib.Parsers.Artwork
{
    class ArtworkHelper
    {

        internal static Bitmap LoadArtworkBitmapFromIThmb(IPod iPod, IPodImageFormat image)
        {
            string iThmbFilename;

            if (image.IsPhoto)
                iThmbFilename = Path.Combine(iPod.FileSystem.PhotoFolderPath, Helpers.iPodPathToStandardPath(image.FileName));
            else
                iThmbFilename = Path.Combine(iPod.FileSystem.ArtworkFolderPath, Helpers.iPodPathToStandardPath(image.FileName));
            
            //string localFileName = Path.GetTempFileName();
            //iPod.FileSystem.CopyFileFromDevice(iThmbFilename, localFileName);
            //Stream fs = new FileStream(localFileName, FileMode.Open);
            Stream fs = iPod.FileSystem.OpenFile(iThmbFilename, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(fs);
            reader.BaseStream.Seek(image.FileOffset, SeekOrigin.Begin);
            byte[] imageData = reader.ReadBytes((int)image.ImageSize);            
            reader.Close();

            //File.Delete(localFileName);
            
            List<SupportedArtworkFormat> supportedFormats = image.IsPhoto ? iPod.DeviceInfo.SupportedPhotoFormats : iPod.DeviceInfo.SupportedArtworkFormats;
            SupportedArtworkFormat supportedFormat = supportedFormats.Find(a => a.FormatId == image.FormatId);

            //if (supportedFormat.FormatId == 3001)
            //{
            //    //UInt16[] nbrs = new UInt16[imageData.Length / 2];
            //    //Buffer.BlockCopy(imageData, 0, nbrs, 0, imageData.Length);
            //    UInt16[] nbrs = new ushort[imageData.Length * 2];
            //    int count = 0;
            //    for (int i = 0; i < imageData.Length; i += 1)
            //    {
            //        nbrs[count] = imageData[count];// BitConverter.ToUInt16(imageData, i);
            //        count++;
            //    }
            //    imageData = unpack_rec_RGB_555(nbrs, nbrs.Length, 1, (int)image.Width, (int)image.Height);
            //}

            Bitmap bitmap;
            if (image.FormatId == 1)
            {
                bitmap = new Bitmap(new MemoryStream(imageData));
            }
            else
            {
                if (supportedFormat == null)
                    return null;

                bitmap = new Bitmap((int)supportedFormat.Width, (int)supportedFormat.Height, supportedFormat.PixelFormat);
                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, (int)supportedFormat.Width, (int)supportedFormat.Height), ImageLockMode.ReadWrite, supportedFormat.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(imageData, 0, bmpData.Scan0, (int)image.ImageSize);
                bitmap.UnlockBits(bmpData);
            }
            
            return bitmap;
        }


        
    
        public const UInt16 ALPHA_BITS_555  = 1;
        public const UInt16  ALPHA_SHIFT_555 =15;
        public const UInt16  ALPHA_MASK_555 = (((1 << ALPHA_BITS_555)-1) << ALPHA_SHIFT_555);

        public const UInt16  RED_BITS_555  = 5;
        public const UInt16 RED_SHIFT_555= 10;
        public const UInt16  RED_MASK_555 = (((1 << RED_BITS_555)-1) << RED_SHIFT_555);

        public const UInt16  GREEN_BITS_555= 5;
        public const UInt16  GREEN_SHIFT_555 =5;
        public const UInt16  GREEN_MASK_555 =(((1 << GREEN_BITS_555)-1) << GREEN_SHIFT_555);

        public const UInt16  BLUE_BITS_555= 5;
        public const UInt16  BLUE_SHIFT_555= 0;
        public const UInt16 BLUE_MASK_555 = (((1 << BLUE_BITS_555) - 1) << BLUE_SHIFT_555);


        static byte[] unpack_rec_RGB_555(UInt16[] pixels, int bytes_len, uint byte_order, int width, int height)
        {
            byte[] result;
            UInt16[] pixels_arranged;

            uint i;

            UInt16[] dd = new UInt16[0];
            pixels_arranged = rearrange_pixels(ref pixels, 0, ref dd, 0, width, height, width);

            if (pixels_arranged == null)
            {
                return null;
            }

            result = new byte[(bytes_len/2) * 3];

            for (i = 0; i < bytes_len/2; i++)
            {
                UInt16 cur_pixel;
                /* FIXME: endianness */
                cur_pixel = pixels_arranged[i];
                /* Unpack pixels */
                result[3 * i] = (byte)((cur_pixel & RED_MASK_555) >> RED_SHIFT_555);
                result[3 * i + 1] = (byte)((cur_pixel & GREEN_MASK_555) >> GREEN_SHIFT_555);
                result[3 * i + 2] = (byte)((cur_pixel & BLUE_MASK_555) >> BLUE_SHIFT_555);

                /* Normalize color values so that they use a [0..255] range */
                result[3 * i] <<= (8 - RED_BITS_555);
                result[3 * i + 1] <<= (8 - GREEN_BITS_555);
                result[3 * i + 2] <<= (8 - BLUE_BITS_555);
            }

            return result;
        }

        static UInt16[] rearrange_pixels(ref UInt16[] pixels_s, int spos, ref UInt16[] pixels_d, int dpos, int width, int height, int row_stride)
        {

            if (width != height)
                return pixels_d;

            if (pixels_d.Length == 0)
            {
                pixels_d = new UInt16[width * height];
            }

            if (width == 1)
            {
                pixels_d[dpos] = pixels_s[spos];
            }
            else
            {
                rearrange_pixels(ref pixels_s, spos + 0,
                ref pixels_d, dpos + (0 + 0),
                width / 2, height / 2,
                row_stride);
                rearrange_pixels(ref pixels_s, spos + ((width / 2) * (height / 2)),
                ref pixels_d, dpos + ((height / 2) * row_stride + 0),
                width / 2, height / 2,
                row_stride);
                rearrange_pixels(ref pixels_s, spos + (2 * (width / 2) * (height / 2)),
                ref pixels_d, dpos + (width / 2),
                width / 2, height / 2,
                row_stride);
                rearrange_pixels(ref pixels_s, spos + (3 * (width / 2) * (height / 2)),
                ref pixels_d, dpos + ((height / 2) * row_stride + width / 2),
                width / 2, height / 2,
                row_stride);
            }

            return pixels_d;

        }



        /*
        internal static void UnpackRgb565(byte[] data, BitmapData dest, bool isbe)
        {
            unsafe
            {
                byte* pixels;
                int row, col;
                ushort s;

                bool flip = isbe == System.BitConverter.IsLittleEndian;

                int offset = 0;
                for (row = 0; row < dest.Height; row++)
                {
                    pixels = ((byte*)dest.Scan0) + row * dest.Stride;
                    for (col = 0; col < dest.Width; col++)
                    {
                        s = BitConverter.ToUInt16(data, offset);
                        offset += 2;

                        if (flip)
                            s = (ushort)((s >> 8) | (s << 8));

                        *(pixels++) = (byte)(((s >> 8) & 0xf8) | ((s >> 13) & 0x7)); // r
                        *(pixels++) = (byte)(((s >> 3) & 0xfc) | ((s >> 9) & 0x3));  // g
                        *(pixels++) = (byte)(((s << 3) & 0xf8) | ((s >> 2) & 0x7));  // b
                    }
                }
            }
        }

        internal static void UnpackIYUV(byte[] data, BitmapData dest)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));

            unsafe
            {
                byte* pixels;
                ushort y0, y1, u, v;
                int r, g, b;
                int row, col;

                for (row = 0; row < dest.Height; row += 2)
                {
                    pixels = ((byte*)dest.Scan0) + row * dest.Stride;
                    for (col = 0; col < dest.Width; col += 2)
                    {
                        u = reader.ReadByte();
                        y0 = reader.ReadByte();
                        v = reader.ReadByte();
                        y1 = reader.ReadByte();

                        UnpackYUV(y0, u, v, out r, out g, out b);
                        *(pixels++) = (byte)r;
                        *(pixels++) = (byte)g;
                        *(pixels++) = (byte)b;

                        UnpackYUV(y1, u, v, out r, out g, out b);
                        *(pixels++) = (byte)r;
                        *(pixels++) = (byte)g;
                        *(pixels++) = (byte)b;
                    }
                }
                for (row = 1; row < dest.Height; row += 2)
                {
                    pixels = ((byte*)dest.Scan0) + row * dest.Stride;
                    for (col = 0; col < dest.Width; col += 2)
                    {
                        u = reader.ReadByte();
                        y0 = reader.ReadByte();
                        v = reader.ReadByte();
                        y1 = reader.ReadByte();

                        UnpackYUV(y0, u, v, out r, out g, out b);
                        *(pixels++) = (byte)r;
                        *(pixels++) = (byte)g;
                        *(pixels++) = (byte)b;

                        UnpackYUV(y1, u, v, out r, out g, out b);
                        *(pixels++) = (byte)r;
                        *(pixels++) = (byte)g;
                        *(pixels++) = (byte)b;
                    }
                }
            }

            reader.Close();
        }

        private static int Clamp(int val, int bottom, int top)
        {
            if (val < bottom)
                return bottom;
            else if (val > top)
                return top;

            return val;
        }

        private static void UnpackYUV(ushort y, ushort u, ushort v, out int r, out int g, out int b)
        {
            r = Clamp((int)(y + (1.370705 * (v - 128))), 0, 255); // r
            g = Clamp((int)(y - (0.698001 * (v - 128)) - (0.3337633 * (u - 128))), 0, 255); // g
            b = Clamp((int)(y + (1.732446 * (u - 128))), 0, 255); // b
        }
        */
        
        public static Bitmap GenerateResizedImage(Image originalImage, SupportedArtworkFormat format)
        {
            try
            {
                Bitmap resizedImage = new Bitmap((int)format.Width, (int)format.Height, format.PixelFormat);

                Graphics gfxContext = Graphics.FromImage(resizedImage);
                gfxContext.CompositingQuality = CompositingQuality.HighQuality;
                gfxContext.SmoothingMode = SmoothingMode.HighQuality;
                gfxContext.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle rect = new Rectangle(0, 0, (int)format.Width, (int)format.Height);
                gfxContext.DrawImage(originalImage, rect);
                return resizedImage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static byte[] GetByteData(Bitmap bmp)
        {
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] bmpBytes = new byte[bitmapData.Stride * bitmapData.Height];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bmpBytes, 0, bmpBytes.Length);
            bmp.UnlockBits(bitmapData);
            return bmpBytes;
        }

        

    }
}
