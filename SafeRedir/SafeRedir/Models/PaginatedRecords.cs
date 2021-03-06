﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Renfield.SafeRedir.Data;

namespace Renfield.SafeRedir.Models
{
  public class PaginatedRecords
  {
    [Display(Name = "Start date (optional)")]
    //[DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = Constants.DATE_FORMAT, ApplyFormatInEditMode = true)]
    public DateTime? FromDate { get; set; }

    [Display(Name = "End date (optional)")]
    //[DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = Constants.DATE_FORMAT, ApplyFormatInEditMode = true)]
    public DateTime? ToDate { get; set; }

    public string DateRange { get; set; }
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }
    public IEnumerable<UrlInfo> UrlInformation { get; set; }
  }
}