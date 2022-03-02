﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Pokerole.Core;
using System.Linq;

namespace WinFormsDEBUG_DexViewer
{
	public partial class GenericItemListForm : Form
	{
		readonly List<IItemBuilder> unbuildable = new List<IItemBuilder>();
		readonly List<Object> builtItems = new List<object>();
		private readonly ImageList smallImageList = new ImageList();
		public GenericItemListForm(Type type, IEnumerable<IItemBuilder> rawList)
		{
			InitializeComponent();
			lstItems.SmallImageList = smallImageList;
			smallImageList.ImageSize = new Size(32, 32);
			ConfigureAndPopulateListForType(type, rawList);
			if (unbuildable.Count == 0)
			{
				lblError.Visible = false;
				btnShowBroken.Visible = false;
			}
			else
			{
				lblError.Visible = true;
				lblError.Text = $"{unbuildable.Count} instance(s) failed to build. Press \"Show Broken Entries\" to see them";
				btnShowBroken.Visible = true;
			}
		}
		public static bool TypeImplemented(Type type)
		{

			switch (type)
			{
				case Type _ when type == typeof(DexEntry.Builder):
					return true;
				case Type _ when type == typeof(Move.Builder):
				case Type _ when type == typeof(ImageRef.Builder):
				case Type _ when type == typeof(Item.Builder):
				case Type _ when type == typeof(Ability.Builder):
				case Type _ when type == typeof(EvolutionList.Builder):
				case Type _ when type == typeof(MonInstance.Builder):
					return false;
			}
			throw new InvalidOperationException($"Unknown type {type}");
		}

		private void ConfigureAndPopulateListForType(Type type, IEnumerable<IItemBuilder> rawList)
		{
			//configure
			lstItems.View = View.Details;
			lstItems.Columns.Clear();
			Func<object, ListViewItem> constructListViewItem;
			Action adjustColumns;
			switch (type)
			{
				case Type _ when type == typeof(DexEntry.Builder):
					lstItems.Columns.AddRange(new ColumnHeader[]
					{
						new ColumnHeader()
						{
							Text = "Dex Num"
						},
						//new ColumnHeader()
						//{
						//	Text = "Sprite"
						//},
						new ColumnHeader()
						{
							Text = "Entry Name"
						}
					});
					rawList = rawList.Cast<DexEntry.Builder>().OrderBy(item => item.DexNum).ThenBy(item => item.Name);
					constructListViewItem = raw =>
					{
						DexEntry entry = (DexEntry)raw;
						//lookup the sprite
						ListViewItem result = new ListViewItem(new String[] { entry.DexNum.ToString(), entry.Name });
						Image? sprite = Form1.LoadImage(entry.SpriteImage);
						if (sprite != null)
						{
							int index = smallImageList.Images.Count;
							smallImageList.Images.Add(sprite);
							result.ImageIndex = index;
						}
						return result;
					};
					adjustColumns = () => lstItems.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
					break;
				case Type _ when type == typeof(ImageRef.Builder):
					//lstItems.Columns.AddRange(new ColumnHeader[]
					//{

					//});




				case Type _ when type == typeof(Move.Builder):
				case Type _ when type == typeof(Item.Builder):
				case Type _ when type == typeof(Ability.Builder):
				case Type _ when type == typeof(EvolutionList.Builder):
				case Type _ when type == typeof(MonInstance.Builder):
					throw new NotImplementedException();
				default: throw new InvalidOperationException($"Unknown type {type}");
			}
			foreach (var item in rawList)
			{
				if (!item.IsValid)
				{
					
					unbuildable.Add(item);
					//throw new NotImplementedException(@"need average height/weight and pokemon category/dex description
					//use pokeapi for height and weight
					//sadness for the other two...
					//also evolution stuff...");
				}
				else
				{
					object builtItem = item.Build();
					lstItems.Items.Add(constructListViewItem(builtItem));
					builtItems.Add(builtItem);
				}
			}
			adjustColumns();
		}

		private void btnShowBroken_Click(object sender, EventArgs e)
		{
			if (unbuildable.Count < 1)
			{
				return;
			}
			using BrokenItemLister lister = new BrokenItemLister(unbuildable);
			lister.ShowDialog(this);
		}

		private void lstItems_ItemActivate(object sender, EventArgs e)
		{
			//for now...
			if (lstItems.SelectedItems.Count != 1)
			{
				return;
			}
			var selected = lstItems.SelectedItems[0];
			using Form temp = new Form()
			{
				Text = "Item Details"
			};
			PropertyGrid propGrid = new PropertyGrid()
			{
				SelectedObject = builtItems[selected.Index],
				Dock = DockStyle.Fill
			};
			temp.Controls.Add(propGrid);
			temp.ShowDialog(this);
		}
	}
}
