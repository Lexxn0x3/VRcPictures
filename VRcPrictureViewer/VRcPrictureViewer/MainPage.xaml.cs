using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using System.Diagnostics;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;

namespace VRcPrictureViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PictureViewer
    {
        //public MainPage()
        //{
        //    this.InitializeComponent();

        //    var task = getPictures();

        //    listViewPics.SelectedIndex = 0;
        //}

        public static async Task<BitmapImage> GetPictureThumbnailFormStorageFile(StorageFile sf)
        {
            var result = await sf.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
            BitmapImage image = new BitmapImage();
            await image.SetSourceAsync(result);

            return image;
        }
        public static async Task<ImageSource> GetPictureFromStorageFile(StorageFile sf)
        {
            using (var randomAccessStream = await sf.OpenAsync(FileAccessMode.Read))
            {
                var result = new BitmapImage();
                await result.SetSourceAsync(randomAccessStream);

                return result;
            }
        }



        //void DeletePic(object sender, RoutedEventArgs e)
        //{
        //    int index = listViewPics.Items.IndexOf(sender);
        //    index = index;
        //}
    }
}
