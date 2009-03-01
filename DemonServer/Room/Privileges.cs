using System;

namespace DemonServer.Room
{
	[Flags]
	public enum SendPrivs : uint
	{
		Images = 1,
		Smilies = 2,
		Emoticons = 4,
		Thumbs = 8,
		Avatars = 16,
		Websites = 32,
		Objects = 64,
		All = Images | Smilies | Emoticons | Thumbs | Avatars | Websites | Objects
	}

	[Flags]
	public enum PrivclassPrivs : uint
	{
		Join = 1,
		ShowNotice = 2,
		Msg = 4,
		Topic = 8,
		Title = 16,
		Kick = 32,
		Admin = 64,
		Default = Join | ShowNotice | Msg
	}
}