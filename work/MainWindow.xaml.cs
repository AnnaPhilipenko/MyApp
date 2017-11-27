using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.ComponentModel;
using NAudio.Wave;

namespace work
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class tags //заполнение playlist, имена столбцов
    {
        public string number { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string year { get; set; }
        public string genre { get; set; }
    }
    public partial class MainWindow : Window
    {
        private List<string> song_list = new List<string>();

        private DirectSoundOut directSoundOut; // воспроизводит пеню
        private AudioFileReader audioFileReader; // хранит экземпляр песни
        enum MediaStatus { None, Stopped, Paused, Running };
        MediaStatus status;
        private int music_pos = 0;
        private bool repeat = false;
        int pl_id = -1; // id выбранного сейчас листа

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);  
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1); 
            dispatcherTimer.Start();
            status = MediaStatus.None;
            pl_update();
        }

        private void pl_update() // Добавление плейлистов в список
        {
            listBox.Items.Clear(); 
            PlayEntities db = new PlayEntities(); // оъект модели бд
            ListViewItem item = new ListViewItem();  
            item.Content = "Default"; // то, что будет написано в item
            item.FontSize = 10; 
            item.FontWeight = FontWeights.Bold;
            listBox.Items.Add(item); 
            foreach (var t in db.playlist) //из бд вытаскивает в список Item
            {
                ListViewItem item1 = new ListViewItem();
                item1.Tag = t.id;
                item1.Content = t.name + ": " + t.songs.Count + " songs";
                listBox.Items.Add(item1);
            }
        }

        private void play(string song_dir) // функция воспроизведения
        {
            status = MediaStatus.Running;
            clear_time();
            Dispose();
            audioFileReader = new AudioFileReader(song_dir);
            directSoundOut = new DirectSoundOut();
            directSoundOut.Init(audioFileReader);
            directSoundOut.Play();
            progress_bar.Maximum = audioFileReader.TotalTime.TotalSeconds; // устанавливается макс зачение
            try
            {
                var audioFile = TagLib.File.Create(song_dir);
                song_name.Content = audioFile.Tag.Title;
                art_name.Content = String.Join(", ", audioFile.Tag.Performers);
            }
            catch (Exception)
            {
                song_name.Content = "";
                art_name.Content = "";
            }
        }

        private void clear_time()
        {
            art_name.Content = "";
            song_name.Content = "";
            time.Content = "00:00";
            current_time.Content = "00:00";
            progress_bar.Value = 0;

        }

        private void update_list()
        {
            Playlist_list.Items.Clear();
            foreach (var t in song_list)
            {
                add_item(t);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            pl_update();
            if (status != MediaStatus.None && status != MediaStatus.Stopped) // преобразовываем 1 в 01
            {
                current_time.Content = String.Format("{0:d2}", audioFileReader.CurrentTime.Minutes) + ":" + String.Format("{0:d2}", audioFileReader.CurrentTime.Seconds);
                time.Content = String.Format("{0:d2}", audioFileReader.TotalTime.Minutes) + ":" + String.Format("{0:d2}", audioFileReader.TotalTime.Seconds);
                progress_bar.Value = audioFileReader.CurrentTime.TotalSeconds;
                if (audioFileReader.Length == audioFileReader.Position) 
                {
                    if (repeat)
                    {
                        play(song_list[music_pos]);
                    }
                    else
                    {
                        if (music_pos < song_list.Count - 1)
                        {
                            music_pos++;
                            play(song_list[music_pos]);
                        }
                        else
                        {
                            music_pos = 0;
                            status = MediaStatus.Stopped;
                            directSoundOut.Stop();
                            clear_time();
                        }
                    }
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e) // нажатие на кнопку стоп
        {
            if (status != MediaStatus.None && status != MediaStatus.Stopped)
            {
                directSoundOut.Stop();
                status = MediaStatus.Stopped;
            }
            progress_bar.Value = 0;
            current_time.Content = "00:00";
        }

        private void PlayOrPause_Click(object sender, RoutedEventArgs e) // нажатие на кнопку воспроизведения
        {
            try
            {
                if (song_list.Count > 0)
                {
                    if (status == MediaStatus.None)
                    {
                        play(song_list[0]);
                    }
                    else if (status == MediaStatus.Running)
                    {
                        directSoundOut.Pause();
                        status = MediaStatus.Paused;
                    }
                    else if (status == MediaStatus.Paused)
                    {
                        directSoundOut.Play();
                        status = MediaStatus.Running;
                    }
                    else if (status == MediaStatus.Stopped)
                    {
                        play(song_list[music_pos]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Dispose() // очистка переменных
        {
            if (directSoundOut != null)
                if (directSoundOut.PlaybackState == PlaybackState.Playing)
                {
                    directSoundOut.Stop();
                    directSoundOut.Dispose();
                    directSoundOut = null;
                }

            if (audioFileReader != null)
                audioFileReader.Dispose();
            audioFileReader = null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }

  

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (directSoundOut != null)
                {
                    audioFileReader.Volume = float.Parse(Volume.Value.ToString());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Аудиофайл не выбран");
            }
        }

        private void add_item(string path) // добавление item в плейлист
        {
            var au = TagLib.File.Create(path); 

            tags song = new tags
            {
                number = (Playlist_list.Items.Count + 1).ToString(),
                title = au.Tag.Title,
                artist = String.Join(", ", au.Tag.Performers),
                album = au.Tag.Album,
                year = au.Tag.Year.ToString(),
                genre = au.Tag.FirstGenre
            };
            var item = new ListViewItem { Content = song, Tag = path }; // Создается item с параметрами
            Playlist_list.Items.Add(item);
        }

        private void Add_Click(object sender, RoutedEventArgs e) // Добавить есню
        {
            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Audio files (.mp3)|*.mp3"; // фильтрует файлы по mp3

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    song_list.Add(filename);
                    add_item(filename);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            if (pl_id != -1) // для недефолтного листа
            {
                db_update(pl_id);
            }
            pl_update();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (song_list.Count > 0)
            {
                if (music_pos < song_list.Count - 1)
                {
                    music_pos++;
                    play(song_list[music_pos]);
                }
                else
                {
                    music_pos = 0;
                    play(song_list[music_pos]);
                }
            }
            else
            {
                MessageBox.Show("Nothing to play", "Error");
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (music_pos > 0)
            {
                music_pos--;
                play(song_list[music_pos]);
            }
        }


        List<string> temp_song_list = new List<string>();
        int temp_music_pos;
        private bool mix = false;
        private void Mix_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!mix)
                {
                    foreach (var t in song_list)
                    {
                        temp_song_list.Add(t);
                    }
                    temp_music_pos = music_pos;

                    for (int i = 0; i < song_list.Count; i++)
                    {
                        int rand = new Random().Next(0, song_list.Count);
                        string temp = song_list[i];
                        song_list[i] = song_list[rand];
                        song_list[rand] = temp;
                        if (i == music_pos)
                        {
                            music_pos = rand;
                        }
                    }
                    update_list();
                    mix = true;
                    Mix.Content = "Shuffle - On";
                }
                else
                {
                    song_list.Clear();
                    foreach (var t in temp_song_list)
                    {
                        song_list.Add(t);
                    }
                    temp_song_list.Clear();
                    update_list();
                    mix = false;
                    music_pos = temp_music_pos;
                    Mix.Content = "Shuffle - Off";
                }
                if (pl_id != -1)
                {
                    db_update(pl_id);
                }
                pl_update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

      

        private void progress_bar_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (status == MediaStatus.Running)
            {
                double u = audioFileReader.Length / audioFileReader.TotalTime.TotalSeconds;
                audioFileReader.Position = Convert.ToInt64(progress_bar.Value * u);
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (!repeat)
            {
                repeat = true;
                Repeat.Content = "Repeat - On";
            }
            else
            {
                repeat = false;
                Repeat.Content = "Repeat - Off";
            }
        }

        private void Playlist_list_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Playlist_list.SelectedItems.Count > 0)
            {
                int g = 0;
                int g1 = 0;
                foreach(var t in song_list) // цикл по всем песням
                {
                    g++;
                    if(t == ((ListViewItem)Playlist_list.SelectedItem).Tag.ToString())  // если выбрана песня,то по тагу в строку
                    {
                        g1 = g;
                    }
                }
                music_pos = g1-1;
                play(song_list[music_pos]);
            }
            if (Playlist_list.SelectedItems.Count != song_list.Count)
            {
                update_list();
                SearchText.Text = "Search";
            }

        }

        private void Remove_Click_1(object sender, RoutedEventArgs e) //удаляет песню из плейлиста
        {
            if (Playlist_list.SelectedItems.Count > 0)
            {

                if (music_pos == Playlist_list.SelectedIndex)
                {
                    if (status != MediaStatus.None && status != MediaStatus.Stopped)
                    {
                        directSoundOut.Stop();
                        status = MediaStatus.Stopped;
                    }
                    clear_time();
                }
                song_list.Remove(song_list[Playlist_list.SelectedIndex]);
                update_list();
                if (pl_id != -1)
                {
                    db_update(pl_id);
                }
                pl_update();
            }
        }

        private void SearchText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string str = SearchText.Text.ToLower();
            try
            {
                if (str != "search")
                {
                    Playlist_list.Items.Clear();
                    if (!String.IsNullOrEmpty(str))
                    {
                        for (int i = 0; i < song_list.Count; i++)
                        {
                            var au = TagLib.File.Create(song_list[i]);

                            string str1 = (au.Tag.Title + String.Join(", ", au.Tag.Performers)).ToLower();
                            if (str1.Contains(str))
                            {
                                add_item(song_list[i]);
                            }
                        }
                    }
                }
                else
                {
                    update_list();
                }
            }
            catch (Exception)
            {

            }

        }

        private void SearchText_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchText.Text = "";
        }

        private void pl_add_Click(object sender, RoutedEventArgs e)
        {
            AddForm p = new AddForm();
            p.ShowDialog();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listBox_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) // список плейлистов
        {
            if (listBox.SelectedItems.Count > 0)
            {
                clear_time();
                if (((ListViewItem)listBox.SelectedItem).Content.ToString() != "Default")
                {
                    int i = Convert.ToInt32(((ListViewItem)listBox.SelectedItem).Tag);
                    pl_id = i;
                    song_list.Clear();

                    PlayEntities db = new PlayEntities();
                    foreach (var t in db.playlist.Find(i).songs)
                    {
                        song_list.Add(t.song);
                    }
                    update_list();
                    playlist_name.Content = db.playlist.Find(i).name;
                    pl_update();
                    if (status != MediaStatus.None && status != MediaStatus.Stopped)
                    {
                        directSoundOut.Stop();
                        status = MediaStatus.Stopped;
                    }
                }
                else
                {
                    pl_id = -1;
                    song_list.Clear();
                    update_list();
                    pl_update();
                    if (status != MediaStatus.None && status != MediaStatus.Stopped)
                    {
                        directSoundOut.Stop();
                        status = MediaStatus.Stopped;
                    }
                }
            }
        }

        private void del_songs(int id) // удаление песен из бд
        {
            PlayEntities db = new PlayEntities();

            var t = db.playlist.Find(id);
            {
                var r = db.songs.Where(x => x.playlist_id == id).ToList(); 
                for (int i = 0; i < r.Count; i++)
                {
                    db.songs.Remove(r[i]);
                }
            }
            db.SaveChanges();
            pl_update();
        }

        private void db_update(int id) // добавление песен в бд
        {
            PlayEntities db = new PlayEntities();
            del_songs(id); 
            foreach (var t in song_list)
            {
                db.playlist.Find(id).songs.Add(new songs { playlist_id = id, song = t });
            }
            db.SaveChanges();
            pl_update();
        }

        private void pl_rem_Click(object sender, RoutedEventArgs e) // удалегте плейлиста
        {
            if (listBox.SelectedItems.Count > 0)
            {
                int i = Convert.ToInt32(((ListViewItem)listBox.SelectedItem).Tag);
                PlayEntities db = new PlayEntities();
                del_songs(i);
                db.playlist.Remove(db.playlist.Find(i));
                db.SaveChanges();
            }
            else
            {
                MessageBox.Show("Select playlist before deleting", "Error");
            }
        }
    }
}
