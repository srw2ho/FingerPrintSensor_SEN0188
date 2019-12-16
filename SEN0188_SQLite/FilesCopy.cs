
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SEN0188_SQLite
{
    public class FileCopy
    {

        public static async Task<String> GetFingerDataBaseFolder()
        {
            try
            {
                String dbPath = "";
                var folder = ApplicationData.Current.LocalFolder;
                var dataBaseFolder = await folder.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists);
                if (dataBaseFolder != null)
                {

                    dbPath = Path.Combine(dataBaseFolder.Path, @"sqliteFinger.db");

                }
                return dbPath;
            }

            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<StorageFile> GetFingerDataBaseStorageFile()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Database");
                StorageFile dbfile = null;

                if (folder != null)
                {
                    dbfile = await folder.GetFileAsync("sqliteFinger.db");
                }

                return dbfile;
            }

            catch (Exception)
            {
                return null;
            }

        }

        public static async Task<String> GetFingerEventDataBaseFolder()
        {
            try
            {
                String dbPath = "";
                var folder = ApplicationData.Current.LocalFolder;
                var dataBaseFolder = await folder.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists);
                if (dataBaseFolder != null)
                {

                    dbPath = Path.Combine(dataBaseFolder.Path, @"sqliteFingerEvent.db");

                }
                return dbPath;
            }

            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<StorageFile> GetFingerEventDataBaseStorageFile()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Database");
                StorageFile dbfile = null;

                if (folder != null)
                {
                    dbfile = await folder.GetFileAsync("sqliteFingerEvent.db");
                }

                return dbfile;
            }

            catch (Exception)
            {
                return null;
            }

        }

        public static async void ImportDataBaseToLocalFolder()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker
                {
                    ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder,
                };

                picker.FileTypeFilter.Add(".db");


                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var folder = ApplicationData.Current.LocalFolder;
                    var dataBaseFolder = await folder.CreateFolderAsync("Database", CreationCollisionOption.OpenIfExists);
                    if (dataBaseFolder != null)
                    {
                        //put file in future access list so it can be accessed when application is closed and reopened
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                        //File is copied to local folder for use in music library
                        if (folder != null && file != null)
                        {
                            await file.CopyAsync(dataBaseFolder, file.Name, NameCollisionOption.ReplaceExisting);
                        }
                    }

                }
            }


            catch (Exception)
            {

            }

        }
        public static async void ExportFingerDataBaseToLocalFolder()
        {
            try
            {
                StorageFile tosaved = await GetFingerDataBaseStorageFile();
                var ret = await ExportStorageFileToLocalFolder(tosaved, "sqliteFinger.db");

                /*

                               if (tosaved != null)
                               {
                                   var picker = new Windows.Storage.Pickers.FileSavePicker();
                                   picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                                   picker.SuggestedFileName = "sqliteSample.db";

                                   var filedb = new[] { ".db" };
                                   picker.FileTypeChoices.Add("DB", filedb);


                                   var file = await picker.PickSaveFileAsync();
                                   if (file != null)
                                   {
                                       await tosaved.CopyAndReplaceAsync(file);


                                   }
                               }
               */

            }

            catch (Exception)
            {

            }

        }

        public static async void ExportFingerEventDataBaseToLocalFolder()
        {
            try
            {
                StorageFile tosaved = await GetFingerEventDataBaseStorageFile();
                var ret = await ExportStorageFileToLocalFolder(tosaved, "sqliteFingerEvent.db");
            }

            catch (Exception)
            {

            }

        }

        public static async Task <bool> ExportStorageFileToLocalFolder(StorageFile tosaved, String suggestFileName)
        {
            try
            {
                if (tosaved != null)
                {
                    var picker = new Windows.Storage.Pickers.FileSavePicker();
                    picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                    picker.SuggestedFileName = suggestFileName;

                    var filedb = new[] { ".db" };
                    picker.FileTypeChoices.Add("DB", filedb);


                    var file = await picker.PickSaveFileAsync();
                    if (file != null)
                    {
                        await tosaved.CopyAndReplaceAsync(file);
                        return true;

                    }
                }
                return false;


            }

            catch (Exception)
            {
                return false;
            }

        }
    }
}

