﻿using System;

namespace SocialNetwork.Library.Services
{
  public class TimeFormatter
  {
    public string Format(TimeSpan timeSpan)
    {
      string unit;
      int value;

      if (timeSpan.Minutes > 0)
      {
        unit = "minute";
        value = timeSpan.Minutes;
      }
      else
      {
        unit = "second";
        value = timeSpan.Seconds;
      }

      var suffix = "";
      if (value != 1)
        suffix = "s";

      return string.Format("{0} {1}{2} ago", value, unit, suffix);
    }
  }
}