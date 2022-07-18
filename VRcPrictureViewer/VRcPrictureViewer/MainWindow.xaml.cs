using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VRcPrictureViewer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var task = getPictures();

            listViewPics.SelectedIndex = 0;
        }

        private void checkboxIDs_Checked(object sender, RoutedEventArgs e)
        {
            textPlayerIDs.Visibility = Visibility.Visible;
            playerIDsHeader.Visibility = Visibility.Visible;
        }
        private void checkboxIDs_Unchecked(object sender, RoutedEventArgs e)
        {
            textPlayerIDs.Visibility = Visibility.Collapsed;
            playerIDsHeader.Visibility = Visibility.Collapsed;
        }

        private async void OpenFileLocation(object sender, RoutedEventArgs e)
        {
            if (listViewPics.SelectedItem == null)
                return;

            Image img = listViewPics.SelectedItem as Image;
            StorageFile file = img.DataContext as StorageFile;

            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(file.Path.Replace(file.Name, ""));

                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception)
            {


            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        async void DeletePic(object sender, RoutedEventArgs e)
        {
            Image img = listViewPics.SelectedItem as Image;
            StorageFile file = img.DataContext as StorageFile;

            DisplayDeleteFileDialog(file);
        }

        private async void DisplayDeleteFileDialog(StorageFile file)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete file permanently?",
                Content = $"If you delete this (selected and shown) file, you won't be able to recover it. Do you want to delete it?\n{file.Name}",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            deleteFileDialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await deleteFileDialog.ShowAsync(ContentDialogPlacement.Popup);
            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                file.DeleteAsync();
                listViewPics.Items.Remove(listViewPics.SelectedItem);
                listViewPics_SelectionChanged(null, null);
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }
        public async Task getPictures()
        {
            StorageFolder folder = null;
            IReadOnlyCollection<StorageFolder> folders = null;

            try
            {
                folders = await KnownFolders.PicturesLibrary.GetFoldersAsync(Windows.Storage.Search.CommonFolderQuery.DefaultQuery);

                foreach (var f in folders)
                {
                    if (f.Name == "VRChat")
                    {
                        folder = f;
                    }
                }

                //var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByDate);
                var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderBySearchRank);
                //var files = await KnownFolders.PicturesLibrary.CreateFileAsync("test.dat");

                foreach (StorageFile file in files)
                {
                    if (file.ContentType.Contains("image"))
                    {
                        Image image1 = new Image();
                        image1.Source = await PictureViewer.GetPictureThumbnailFormStorageFile(file);
                        image1.DataContext = file;
                        listViewPics.Items.Add(image1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageDialog requestPermissionDialog = new MessageDialog($"The app needs to access to your Pictures. " +
                      "\nGrant access in the system settings and give this app permission. " +
                      "\nIf the app closes, please reopen it afterwards. " +
                      "\nIf you Cancel, the app will have limited functionality only.");

                requestPermissionDialog.ShowAsync();
                Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-pictures"));
            }
        }
        async void MainPage_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
        {
            try
            {
                if (listViewPics.SelectedItem == null)
                    return;

                Image img = listViewPics.SelectedItem as Image;
                StorageFile file = img.DataContext as StorageFile;

                DataPackage requestData = args.Request.Data;
                requestData.Properties.Title = "Your Title";
                requestData.Properties.Description = "Your Description";

                List<IStorageItem> imageItems = new List<IStorageItem> { file };
                requestData.SetStorageItems(imageItems);

                RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
                requestData.Properties.Thumbnail = imageStreamRef;
                requestData.SetBitmap(imageStreamRef);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, "ERROR").ShowAsync();
            }
        }

        async void Share(object sender, RoutedEventArgs e)
        {
        }
        private async void listViewPics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewPics.SelectedItem == null)
                return;

            Image img = listViewPics.SelectedItem as Image;
            StorageFile file = img.DataContext as StorageFile;

            progressRingPic.IsActive = true;
            imageViewer.Source = await PictureViewer.GetPictureThumbnailFormStorageFile(file);

            try
            {
                imageViewer.Source = await PictureViewer.GetPictureFromStorageFile(file);
            }
            catch (Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog(ex.Message);
                messageDialog.ShowAsync();
                listViewPics.Items.Remove(listViewPics.SelectedItem);
            }

            progressRingPic.IsActive = false;

            ImageProperties props = await file.Properties.GetImagePropertiesAsync();
            BasicProperties basicProps = await file.GetBasicPropertiesAsync();

            textBlockBitmapSize.Text = ((ulong)((ulong)props.Width * (ulong)props.Height * 24 * 3) / (ulong)8000000).ToString() + "mb";
            textBlockResolution.Text = $"{props.Width}x{props.Height}";
            textBlockFileSize.Text = Math.Round((double)((double)basicProps.Size / 1049000), 0) + "mb";
            textBlockFileName.Text = file.DisplayName;
            textBlockDateTaken.Text = file.DateCreated.ToString();

            List<string> PropertiesToGet_RW = new List<string>()
        {
            "System.Title",
            "System.Comment",
            "System.DateCreated",
            "System.DateModified",
        };

            IDictionary<string, object> retrievedProps = await props.RetrievePropertiesAsync(PropertiesToGet_RW);


            VrcProperties vrcProperties = new VrcProperties(retrievedProps);

            textBlockPlayers.Text = "";
            textPlayerIDs.Text = "";

            if (vrcProperties.Available)
            {
                textBlockWorld.Text = vrcProperties.World.Name;
                textBlockWorldID.Text = vrcProperties.World.ID;
                textBlockTakenBy.Text = vrcProperties.Author.Name;
                textBlockPosition.Text = vrcProperties.Position;
                textBlockRotation.Text = vrcProperties.Orientation;

                for (int i = 0; i < vrcProperties.PlayersOnPic.Count; i++)
                {
                    if (i == vrcProperties.PlayersOnPic.Count - 1)
                    {
                        textBlockPlayers.Text += vrcProperties.PlayersOnPic[i].Name;
                        textPlayerIDs.Text += vrcProperties.PlayersOnPic[i].ID;
                    }
                    else
                    {
                        textBlockPlayers.Text += vrcProperties.PlayersOnPic[i].Name + "\n";
                        textPlayerIDs.Text += vrcProperties.PlayersOnPic[i].ID + "\n";
                    }
                }
            }
            else
            {
                textBlockPlayers.Text = "N.A";
                textBlockWorld.Text = "N.A";
                textBlockWorldID.Text = "N.A";
                textBlockTakenBy.Text = "N.A";
                textBlockPosition.Text = "N.A";
                textBlockRotation.Text = "N.A";
                textPlayerIDs.Text = "N.A";
            }
        }
    }
}

