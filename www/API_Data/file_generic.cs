
namespace AnatomyNext.WebGLApp.Www.API_Data
{
    public class file_generic
    {
        public int ID;
        public string Title;
        public string Filename;
        public string FileURL;
        public int Version = 0;

        public override string ToString()
        {
            return string.Format("{0} - {1}", ID, FileURL);
        }
    }
}
