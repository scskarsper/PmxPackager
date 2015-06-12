using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace PMXPackager
{
    class ImageLoader
    {
        public static Image Load(string File)
        {
            Image Ret = null;
            try
            {
                Ret = Image.FromFile(File);
            }
            catch { Ret=null;}
            if (Ret == null)
            {
                Bitmap bmp = new Bitmap(100, 100);
                bool b=TGAHelper.OpenTGAFile(new FileStream(File, FileMode.Open), ref bmp);
                if (b) Ret = bmp;
            }
            return Ret;
        }
    }
    class TGAHelper
    {
        struct sTGAHEADER
        {
            //TGA文件头结构 
            public byte id_length;
            public byte colormap_type;
            public byte image_type;
            public ushort colormap_index;
            public ushort colormap_length;
            public byte colormap_size;
            public ushort x;
            public ushort y;
            public ushort width;
            public ushort heigth;
            public byte pixel_size;
            public byte attributes;
        }

        //读取TGA文件的函数，将图象解码后写入内存Bitmap类型中 
        public static bool OpenTGAFile(FileStream tagContent, ref Bitmap bmp)
        {
            sTGAHEADER th;
            FileStream fs = tagContent;//new MemoryStream(tagContent);
            BinaryReader br = new BinaryReader(fs);
            try
            {
                //读取文件头，因为我不知道如何从流中读取自定义数据结构，只好一个一个读了 
                th.id_length = br.ReadByte();
                th.colormap_type = br.ReadByte();
                th.image_type = br.ReadByte();
                th.colormap_index = br.ReadUInt16();
                th.colormap_length = br.ReadUInt16();
                th.colormap_size = br.ReadByte();
                th.x = br.ReadUInt16();
                th.y = br.ReadUInt16();
                th.width = br.ReadUInt16();
                th.heigth = br.ReadUInt16();
                th.pixel_size = br.ReadByte();
                th.attributes = br.ReadByte();

                if (th.pixel_size != 24 & th.pixel_size != 32)
                {
                    fs.Close();
                    br.Close();
                    return false;
                }

                int x;
                int y;
                int dest;
                byte[] p = new byte[4];
                //读取颜色值的临时变量 
                byte[] p24;

                fs.Seek(th.id_length, SeekOrigin.Current);
                //定位文件指针跳过文件信息 

                int ByteCount = 4 * th.width * th.heigth;
                //目标始终是32位格式 
                byte[] Bytes = new byte[ByteCount];
                //分配临时内存 

                if (th.image_type == 2)
                {
                    //不压缩格式，直接读入每个象素的颜色值 
                    dest = 0;
                    for (y = th.heigth - 1; y >= 0; y += -1)
                    {
                        //图象是上下倒置的 
                        dest = y * th.width * 4;
                        for (x = 0; x <= th.width - 1; x++)
                        {
                            if (th.pixel_size == 24)
                            {
                                p24 = br.ReadBytes(3);
                                //24位读入3个字节, 它会改变数组P的维数 
                                p[0] = p24[0];
                                p[1] = p24[1];
                                p[2] = p24[2];
                                p[3] = 255;
                            }
                            else
                            {
                                p = br.ReadBytes(4);
                                //32位格式，读入4个字节 
                            }
                            Bytes[dest] = p[0];
                            Bytes[dest + 1] = p[1];
                            Bytes[dest + 2] = p[2];
                            Bytes[dest + 3] = p[3];

                            dest += 4;
                        }
                    }
                }
                else if (th.image_type == 10)
                {
                    //RLE压缩 
                    //TGA文件RLE压缩的方式为，从最下一行向上记录，块标识如果大于127，后面为一个颜色值， 
                    //表示之后的标识减去127个象素都是这个颜色；如果标识小于128，则后面标识数量的象素为没有压缩的颜色值。 
                    byte PacketHeader;
                    int PacketSize;
                    int a;
                    int i;
                    int j;

                    j = (int)th.width * (int)th.heigth;
                    i = 0;
                    do
                    {
                        PacketHeader = br.ReadByte();
                        //读入一个字节 
                        if (PacketHeader >= 128)
                            PacketSize = (PacketHeader - 128);
                        else
                            PacketSize = PacketHeader;
                        for (a = 0; a <= PacketSize; a++)
                        {
                            //循环块 
                            if (PacketHeader < 128 | a == 0)
                            {
                                //不是压缩块，每次都要读入数值 
                                if (th.pixel_size == 24)
                                {
                                    p24 = br.ReadBytes(3);
                                    //24位读入3个字节, 它会改变数组P的维数 
                                    p[0] = p24[0];
                                    p[1] = p24[1];
                                    p[2] = p24[2];
                                    p[3] = 255;
                                }
                                else
                                {
                                    p = br.ReadBytes(4);
                                    //32位格式，读入4个字节 
                                }
                            }
                            y = th.heigth - (i / th.width) - 1;
                            x = i % th.width;
                            dest = (y * th.width + x) * 4;
                            Bytes[dest] = p[0];
                            Bytes[dest + 1] = p[1];
                            Bytes[dest + 2] = p[2];
                            Bytes[dest + 3] = p[3];

                            i += 1;
                            if (i == j)
                                break; // TODO: might not be correct. Was : Exit Do 

                        }
                    }
                    while (true);
                }
                else
                {
                    fs.Close();
                    br.Close();
                    return false;
                }
                //所有数据已经读入bytes，创建离屏表面 
                if ((bmp != null))
                {
                    //释放之前的内存 
                    bmp.Dispose();
                    bmp = null;
                }
                bmp = new Bitmap(th.width, th.heigth, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData;

                //锁定内存 
                bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

                //得到内存指针
                IntPtr ptr = bmpData.Scan0;

                //拷贝数据到指针内存 
                System.Runtime.InteropServices.Marshal.Copy(Bytes, 0, ptr, ByteCount);

                //解锁 
                bmp.UnlockBits(bmpData);

                //翻转
                bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                fs.Close();
                br.Close();
                return true;
            }
            catch { fs.Close(); return false; ;}
        }
    }
}
