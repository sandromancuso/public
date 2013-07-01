﻿using Renfield.Inventory.Data;

namespace Renfield.Inventory.Models
{
  public class AcquisitionItemModel
  {
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string Quantity { get; set; }
    public string Price { get; set; }
    public string Value { get; set; }

    public static AcquisitionItemModel From(AcquisitionItem value)
    {
      return new AcquisitionItemModel
      {
        Id = value.Id,
        ProductName = value.Product.Name,
        Quantity = value.Quantity.Formatted(),
        Price = value.Price.Formatted(),
        Value = (value.Quantity * value.Price).Formatted(),
      };
    }
  }
}