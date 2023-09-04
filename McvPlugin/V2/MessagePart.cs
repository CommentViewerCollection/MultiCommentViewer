﻿namespace Mcv.PluginV2;

public interface IMessagePart { }
public interface IMessageEmpty : IMessagePart { }
public interface IMessageText : IMessagePart
{
    string Text { get; }
}
public interface IMessageImage : IMessagePart
{
    int? X { get; }
    int? Y { get; }
    int? Width { get; }
    int? Height { get; }
    string Url { get; }

    string Alt { get; }
}
/// <summary>
/// 指定された画像の一部を描画する用
/// </summary>
public interface IMessageImagePortion : IMessagePart
{
    int SrcX { get; }
    int SrcY { get; }
    int SrcWidth { get; }
    int SrcHeight { get; }
    /// <summary>
    /// 表示時の幅
    /// </summary>
    int Width { get; }
    /// <summary>
    /// 表示時の高さ
    /// </summary>
    int Height { get; }
    System.Drawing.Image Image { get; }
    string Alt { get; }
}
public interface IMessageLink : IMessageText
{
    string Url { get; }
}
public interface IMessageRemoteSvg : IMessagePart
{
    int? Width { get; }
    int? Height { get; }
    string Url { get; }
    string Alt { get; }
}
public interface IMessageSvg : IMessagePart
{
    int? Width { get; }
    int? Height { get; }
    string Data { get; }
    string Alt { get; }
}
