﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BleemSync.Data.Models;
using BleemSync.Services;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaApplication1.Views
{
    public class GameManager : UserControl
    {
        private class PageViewModel : ReactiveObject
        {
            private ObservableCollection<string> items;

            public ObservableCollection<string> Items {
                get => items;
                set => this.RaiseAndSetIfChanged(ref items, value);
            }

            public PageViewModel(ICollection<Game> games)
            {
                Items = new ObservableCollection<string>(games.Select(g => g.Title));
            }
        }

        private GameService GameService { get; set; }
        private PageViewModel ViewModel { get; set; }

        public GameManager()
        {
            InitializeComponent();

            GameService = new GameService();

            ViewModel = new PageViewModel(GameService.Get().ToList());
            DataContext = ViewModel;

            AddHandler(DragDrop.DropEvent, Drop);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None;
            }
            else
            {
                e.DragEffects = DragDropEffects.Link;

                foreach (var file in e.Data.GetFileNames())
                {
                    var fingerprintService = new FingerprintService();
                    var scraperService = new ScraperService();
                    
                    string fingerprint = fingerprintService.GetFingerprint(file);

                    var game = scraperService.ScrapeGame(fingerprint);

                    game.Path = file;

                    GameService.Add(game);

                    ViewModel.Items.Add(game.Title);
                }
            }
        }
    }
}
