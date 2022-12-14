using Hockey.Client.Main.Model.Abstraction;
using Hockey.Client.Shared.Extensions;
using Microsoft.Win32;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace Hockey.Client.Main.ViewModel;

internal class MainViewModel : ReactiveObject
{
    public IMainModel Model { get; }

    [Reactive] public ImageSource Frame { get; set; }
    [Reactive] public CancellationTokenSource LastTokenSource { get; set; }

    public ICommand ReadVideoFromFileCommand { get; }
    public ICommand ReversePauseCommand { get; }

    public ICommand UserClickedCommand { get; }
    public ICommand StopVideoCommand { get; }

    public MainViewModel(IMainModel model)
    {
        Model = model;

        Model.WhenAnyValue(x => x.CurrentFrame)
             .Select(x => x is null || x.IsDisposed ? default : BitmapSourceConverter.ToBitmapSource(x))
             .Subscribe(x => Frame = x)
             .Cache();

        ReadVideoFromFileCommand = ReactiveCommand.Create
        (
            async () =>
            {
                OpenFileDialog openFileDialog = new();

                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }

                LastTokenSource?.Cancel();
                LastTokenSource = new();
                await Model.ReadVideoFromFile(openFileDialog.FileName, LastTokenSource.Token);
            }
        );

        StopVideoCommand = ReactiveCommand.Create(() => LastTokenSource.Cancel());

        ReversePauseCommand = ReactiveCommand.Create(() => Model.IsPaused = !Model.IsPaused);
        UserClickedCommand = ReactiveCommand.Create<bool>(x => Model.IsUserClick = x);
    }
}
