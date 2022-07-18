using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace VRcPrictureViewer
{
    internal class VrcProperties
    {
        public Entity World { get; }
        public Entity Author { get; }
        public string Position { get; }
        public List <Entity> PlayersOnPic { get; }
        public string Orientation { get; }
        public bool Available { get; }

        public VrcProperties(IDictionary<string, object> retrievedProps)
        {
            
            PlayersOnPic = new List<Entity>();


            Dictionary<string, string> valuePairs = new Dictionary<string, string>();

            if (retrievedProps["System.Comment"] == null)
            {
                Available = false;
                return;
            }

            string comment = retrievedProps["System.Comment"].ToString();
            string[] properties = comment.Split('|');

            if (properties.Length > 5)
                Available = true;

            foreach (var property in properties)
            {
                int indexPropName = property.IndexOf(':');
                if (indexPropName == -1)
                    continue;

                valuePairs.Add(property.Substring(0, indexPropName), property.Substring(indexPropName + 1));
                indexPropName = -1;
            }

            if (valuePairs.Keys.Contains("author"))
            {
                string[] content = valuePairs["author"].Split(',');
                if (content.Length == 2)
                {
                    Author = new Entity(content[0], content[1]);
                }
                else
                    Available = false;

            }
            if (valuePairs.Keys.Contains("world"))
            {
                string[] content = valuePairs["world"].Split(',');
                if (content.Length == 3)
                {
                    World = new Entity(content[0], content[2]);
                }
                else
                    Available = false;

            }
            if (valuePairs.Keys.Contains("pos"))
            {
                Position = valuePairs["pos"];
            }
            else
                Position = "";
            if (valuePairs.Keys.Contains("rq"))
            {
                Orientation = valuePairs["rq"];
            }
            else
                Orientation = "Normal";
            if (valuePairs.Keys.Contains("players"))
            {
                string[] players = valuePairs["players"].Split(';');

                foreach (string player in players)
                {
                    string[] data = player.Split(',');
                    if (data.Length == 5)
                        PlayersOnPic.Add(new Entity(data[0], data[4]));
                    else if (PlayersOnPic.Count == 0)
                        Available = false;
                }
            }
        }

        public struct Entity
        {
            public string ID { get;}
            public string Name { get;}

            public Entity(string id, string name)
            {
                ID = id;
                Name = name;
            }
        }
    }
}
