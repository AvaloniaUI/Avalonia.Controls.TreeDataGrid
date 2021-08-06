using System.Collections.Generic;

namespace PlasticGui
{
    public class PlasticTableColumn
    {
        public enum Sorting { Ascending, Descending }
        public enum Type { String, Date, Number, RepositoryName, SecurityMember, Seid }
        public enum Render { Text, IconAndText, CheckBoxAndText, CheckBoxIconAndText }

        public readonly string Name;
        public readonly object Content;
        public readonly int DefaultWidth;
        public readonly Render RenderType;

        public PlasticTableColumn(string name, int defaultWidth)
            : this(name, defaultWidth, Render.Text)
        {
        }

        public PlasticTableColumn(string name, int defaultWidth, Render renderType)
            : this(name, defaultWidth, renderType, null)
        {
        }

        public PlasticTableColumn(
            string name,
            int defaultWidth,
            Render renderType,
            object content)
        {
            Name = name;
            DefaultWidth = defaultWidth;
            RenderType = renderType;
            Content = content;
        }

        internal static List<string> GetColumnNames(PlasticTableColumn[] columns)
        {
            List<string> result = new List<string>();
            foreach (PlasticTableColumn column in columns)
            {
                result.Add(column.Name);
            }
            return result;
        }
    }

    public class PlasticTableCell
    {
        public enum ImageType { None, Generic, Repository, DeletedRepository, User, Group, Owner }
        public enum StyleType { Normal, Lighter }
        public enum ColorType { Regular, Deleted }

        public readonly string Text;

        public ImageType Image;
        public StyleType Style;
        public ColorType Color;

        public PlasticTableCell(string text)
            : this(text, ImageType.None, StyleType.Normal, ColorType.Regular)
        {
        }

        public PlasticTableCell(string text, ImageType image)
            : this(text, image, StyleType.Normal, ColorType.Regular)
        {
        }

        public PlasticTableCell(string text, ImageType image, StyleType style)
            : this(text, image, style, ColorType.Regular)
        {
        }

        public PlasticTableCell(
            string text, ImageType image, StyleType style, ColorType color)
        {
            Text = text;
            Image = image;
            Style = style;
            Color = color;
        }
    }
}