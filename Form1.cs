using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PMDEditor;

namespace PMXPackager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        PmxFile Loader = new PmxFile();
        Pmx tab1_PModel = null;
        string tab1_PmxFilePath = "";
        private void tab1_OpenPMX_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "打开MMD模型";
            ofd.AddExtension = true;
            ofd.DefaultExt = ".pmx";
            ofd.Filter = "*.pmx|*.pmx";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tab1_PmxFile.Text = ofd.FileName;
                tab1_LoadPmd(ofd.FileName);
            }
        }
        
        void tab1_LoadPmd(string FileName)
        {
            tab1_PModel = Loader.GetFile(FileName);
            tab1_PmxFilePath = FileName;
            System.IO.FileInfo fi = new System.IO.FileInfo(FileName);
            string BaseDir = fi.Directory.FullName;
            listBox1.Items.Clear();
            foreach(PmxMaterial pxm in tab1_PModel.MaterialList)
            {
                string Tex = pxm.Tex;
                string Toon = pxm.Toon;
                string Spa = pxm.Sphere;
                if (Tex != "")
                {
                    string F=GenRealDir(BaseDir, Tex);
                    if (System.IO.File.Exists(F))
                    {
                        string K = "[Tex]" + F;
                        if (!listBox1.Items.Contains(K))
                        {
                            listBox1.Items.Add(K);
                        }
                    }
                }
                if (Toon != "")
                {
                    string F=GenRealDir(BaseDir, Toon);
                    if (System.IO.File.Exists(F))
                    {
                        string K = "[Toon]" + F;
                        if (!listBox1.Items.Contains(K))
                        {
                            listBox1.Items.Add(K);
                        }
                    }
                }
                if (Spa != "")
                {
                    string F=GenRealDir(BaseDir, Spa);
                    if (System.IO.File.Exists(F))
                    {
                        string K = "[Spa]" + F;
                        if (!listBox1.Items.Contains(K))
                        {
                            listBox1.Items.Add(K);
                        }
                    }
                }
            }
        }

        static string GenRealDir(string BaseDir, string Dir)
        {
            if (Dir.Length > 1 && Dir[1] == ':')
            {
                return Dir;
            }
            string K = BaseDir + "\\" + Dir;
            return K.Replace("\\\\", "\\");
        }
        static string GenRetDir(string BaseDir, string Dir)
        {
            return Dir.Replace(BaseDir, "");
        }

        string CopyPmxFile(string SourceFile, string TargetDir,string Type)
        {
            Type = Type.ToLower();
            try
            {
                if (!System.IO.Directory.Exists(TargetDir+"\\"+Type))
                {
                    System.IO.Directory.CreateDirectory(TargetDir + "\\" + Type);
                }
            }
            catch { ;}
            System.IO.FileInfo fi = new System.IO.FileInfo(SourceFile);
            string Fn = fi.Name.ToLower();
            string Fnn = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length).ToLower();
            string TFn = Fn.ToLower();
            #region 图像格式转换
            if (formatType.Checked)
            {
                Image Img = ImageLoader.Load(SourceFile);
                if (Img != null)
                {
                    string Ext = "";
                    System.Drawing.Imaging.ImageFormat Fmt = GetTab1Format(ref Ext);
                    if (Fmt == null)
                    {
                        System.IO.File.Copy(SourceFile, TargetDir + "\\" + Type + "\\" + TFn, true);
                    }
                    else
                    {
                        TFn = Fnn + "." + Ext;
                        Img.Save(TargetDir + "\\" + Type + "\\" + TFn, Fmt);
                    }
                }
                else
                {
                    System.IO.File.Copy(SourceFile, TargetDir + "\\" + Type + "\\" + TFn, true);
                }
            }
            else
            {
                System.IO.File.Copy(SourceFile, TargetDir + "\\" + Type + "\\" + TFn, true);
            }
            #endregion
            string Ret=Type + "\\" + TFn;
            while(Ret[0]=='\\')
            {
                Ret = Ret.Substring(1);
            }
            return Ret;
        }
        System.Drawing.Imaging.ImageFormat GetTab1Format(ref string Ext)
        {
            switch (cmb_FormatType.Text)
            {
                case "Bmp": Ext = "bmp"; return System.Drawing.Imaging.ImageFormat.Bmp;
                case "Emf": Ext = "emf"; return System.Drawing.Imaging.ImageFormat.Emf;
                case "Gif": Ext = "gif"; return System.Drawing.Imaging.ImageFormat.Gif;
                case "Jpeg": Ext = "jpg"; return System.Drawing.Imaging.ImageFormat.Jpeg;
                case "Png": Ext = "png"; return System.Drawing.Imaging.ImageFormat.Png;
                case "Tiff": Ext = "tiff"; return System.Drawing.Imaging.ImageFormat.Tiff;
                case "Wmf": Ext = "wmf"; return System.Drawing.Imaging.ImageFormat.Wmf;
                default: return null;
            }
        }
        private void tab1_Format_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择PMX文件格式化输出的目录，最好为空目录";
            fbd.ShowNewFolderButton = true;
            System.IO.FileInfo fi = new System.IO.FileInfo(tab1_PmxFilePath);
            string BaseDir = fi.Directory.FullName;
            fbd.SelectedPath = BaseDir;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string TargetDir = fbd.SelectedPath;
                for (int i = 0; i < tab1_PModel.MaterialList.Count;i++ )
                {
                    string Tex = tab1_PModel.MaterialList[i].Tex;
                    string Toon = tab1_PModel.MaterialList[i].Toon;
                    string Spa = tab1_PModel.MaterialList[i].Sphere;
                    if (Tex != "")
                    {
                        string AT = "tex";
                        if (checkBox4.Checked) AT = "";
                        string F = GenRealDir(BaseDir, Tex);
                        if (System.IO.File.Exists(F))
                        {
                            tab1_PModel.MaterialList[i].Tex = CopyPmxFile(F, TargetDir, AT);
                        }
                        else
                        {
                            if (checkBox1.Checked)
                            {
                                string File = System.IO.Path.GetTempPath() + "\\Color_" + tab1_PModel.MaterialList[i].Diffuse.ToArgb().ToString() + ".jpg";
                                if (!System.IO.File.Exists(File))
                                {
                                    Color NewColor = Color.FromArgb(tab1_PModel.MaterialList[i].Diffuse.ToArgb());
                                    Bitmap bmp = new Bitmap(10, 10);
                                    Graphics g = Graphics.FromImage(bmp);
                                    g.FillRectangle(new SolidBrush(NewColor), -100, -100, 200, 200);
                                    g.Dispose();
                                    bmp.Save(File, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }
                                tab1_PModel.MaterialList[i].Tex = CopyPmxFile(File, TargetDir, AT);
                            }
                            else
                            {
                                tab1_PModel.MaterialList[i].Tex = "";
                            }
                        }
                    }
                    else
                    {
                        string AT = "tex";
                        if (checkBox4.Checked) AT = "";
                        if (checkBox1.Checked)
                        {
                            string File = System.IO.Path.GetTempPath() + "\\Color_" + tab1_PModel.MaterialList[i].Diffuse.ToArgb().ToString() + ".jpg";
                            if (!System.IO.File.Exists(File))
                            {
                                Color NewColor = Color.FromArgb(tab1_PModel.MaterialList[i].Diffuse.ToArgb());
                                Bitmap bmp = new Bitmap(10, 10);
                                Graphics g = Graphics.FromImage(bmp);
                                g.FillRectangle(new SolidBrush(NewColor), -100, -100, 200, 200);
                                g.Dispose();
                                bmp.Save(File, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            tab1_PModel.MaterialList[i].Tex = CopyPmxFile(File, TargetDir, AT);
                        }
                    }
                    if (Toon != "")
                    {
                        if (checkBox3.Checked)
                        {
                            tab1_PModel.MaterialList[i].Toon = "";
                        }
                        else
                        {
                            string AT = "toon";
                            if (checkBox4.Checked) AT = "";
                            string F = GenRealDir(BaseDir, Toon);
                            if (System.IO.File.Exists(F))
                            {
                                tab1_PModel.MaterialList[i].Toon = CopyPmxFile(F, TargetDir, AT);
                            }
                            else
                            {
                                tab1_PModel.MaterialList[i].Toon = "";
                            }
                        }
                    }
                    if (Spa != "")
                    {
                        if (checkBox2.Checked)
                        {
                            tab1_PModel.MaterialList[i].Sphere = "";
                            tab1_PModel.MaterialList[i].SphereMode = PmxMaterial.SphereModeType.None;
                        }
                        else
                        {
                            string AT = "sph";
                            if (checkBox4.Checked) AT = "";
                            string F = GenRealDir(BaseDir, Spa);
                            if (System.IO.File.Exists(F))
                            {
                                tab1_PModel.MaterialList[i].Sphere = CopyPmxFile(F, TargetDir, AT);
                            }
                            else
                            {
                                tab1_PModel.MaterialList[i].Sphere = "";
                            }
                        }
                    }
                }
                tab1_PModel.ToFile(TargetDir + "\\" + fi.Name);
                tab1_LoadPmd(tab1_PmxFilePath);
                MessageBox.Show("整理完成！");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string K = (string)listBox1.SelectedItem;
                string BK = K.Substring(0, 5);
                switch (BK)
                {
                    case "[Toon": K = K.Substring(6); break;
                    case "[Tex]": K = K.Substring(5); break;
                    case "[Spa]": K = K.Substring(5); break;
                }
                pictureBox1.Image = ImageLoader.Load(K);// Image.FromFile(K);
            }
            catch { ;}
        }
    }
}
