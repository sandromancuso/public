﻿using System.Collections.Generic;

namespace Renfield.VideoSpinner.Library
{
    public interface Shuffler
    {
        IList<T> Shuffle<T>(IList<T> list);

        IEnumerable<double> GetRandomizedDurations(double duration, int count);
    }
}