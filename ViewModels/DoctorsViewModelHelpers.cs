using System;

namespace LiteClinic.ViewModels
{
    internal static class DoctorsViewModelHelpers
    {

        private static readonly string[] DefaultAvatars =
        {
    "ms-appx:///Assets/Profiles/Defaults/male_avatar.png",
    "ms-appx:///Assets/Profiles/Defaults/female_avatar.png",
    "ms-appx:///Assets/Profiles/Defaults/female_avatar_hejab.png"
};

        private static string GetRandomDefaultAvatar()
        {
            var random = new Random();
            int index = random.Next(DefaultAvatars.Length);
            return DefaultAvatars[index];
        }
    }
}