using Mawi.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WGrid : WContainer
    {
        private RowDefinitionCollection _rowDefinitions { get; set; }
        private ColumnDefinitionCollection _columnDefinitions { get; set; }

        public RowDefinitionCollection RowDefinitions { get => _rowDefinitions; set { _rowDefinitions = value; MarkUpdated(); } }
        public ColumnDefinitionCollection ColumnDefinitions { get => _columnDefinitions; set { _columnDefinitions = value; MarkUpdated(); } }

        private WLength _rowSpacing;
        private WLength _columnSpacing;
        public WLength RowSpacing { get => _rowSpacing; set { _rowSpacing = value; MarkUpdated(); } }
        public WLength ColumnSpacing { get => _columnSpacing; set { _columnSpacing = value; MarkUpdated(); } }

        public string RowDefinition { get => string.Empty; set {
                RowDefinitions = ParseRowDefinitions(value);
            } 
        }

        public string ColumnDefinition {
            get => string.Empty; set
            {
                ColumnDefinitions = ParseColumnDefinitions(value);
            }
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            sb.Append("justify-self:");
            switch (child.HorizontalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.WidthRequest == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(";");

            sb.Append("align-self:");
            switch (child.VerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.HeightRequest == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(";");

            var settings = child.LayoutSettings as WGridLayoutSettings;
            if (settings == null)
            {
                settings = new WGridLayoutSettings();
                //TODO: see flexlayout for same issue
                //child.LayoutSettings = settings;
            }

            var colplusone = settings.Column + 1;
            var rowplusone = settings.Row + 1;

            sb.Append("grid-row: ").Append(rowplusone);
            if (settings.RowSpan != 1)
            {
                sb.Append(" / ").Append(rowplusone + settings.RowSpan);
            }
            sb.Append(";");


            sb.Append("grid-column: ").Append(colplusone);
            if (settings.ColumnSpan != 1)
            {
                sb.Append(" / ").Append(colplusone + settings.ColumnSpan);
            }
            sb.Append(";");
        }
        
        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:grid;");            

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            if (RowDefinitions != null)
            {
                sb.Append("grid-template-rows:");
                foreach (var row in RowDefinitions)
                {
                    sb.Append(" ");
                    if (row.Height.GridUnitType == GridUnitType.Auto)
                    {
                        sb.Append("auto");
                    }
                    else
                    {
                        sb.Append(row.Height.Value.ToInv());
                        switch (row.Height.GridUnitType)
                        {
                            case GridUnitType.Star:
                                sb.Append("fr");
                                break;

                            case GridUnitType.Absolute:
                                sb.Append("px");
                                break;
                        }
                    }
                }
                sb.Append(';');
            }
            else
            {
                sb.Append("grid-template-rows:1fr;");
            }

            if (ColumnDefinitions != null)
            {
                sb.Append("grid-template-columns:");
                foreach (var col in ColumnDefinitions)
                {
                    sb.Append(" ");
                    if (col.Width.GridUnitType == GridUnitType.Auto)
                    {
                        sb.Append("auto");
                    }
                    else
                    {
                        sb.Append(col.Width.Value.ToInv());
                        switch (col.Width.GridUnitType)
                        {
                            case GridUnitType.Star:
                                sb.Append("fr");
                                break;

                            case GridUnitType.Absolute:
                                sb.Append("px");
                                break;
                        }
                    }
                }
                sb.Append(';');
            }
            else
            {
                sb.Append("grid-template-columns:1fr;");
            }

            if (_rowSpacing.Value != 0)
            {
                sb.Append("row-gap:");
                _rowSpacing.Write(sb);
                sb.Append(';');
            }

            if (_columnSpacing.Value != 0)
            {
                sb.Append("column-gap:");
                _columnSpacing.Write(sb);
                sb.Append(';');
            }

            Parent.RenderChildLayout(this, sb);
        }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-grid");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);


            sb.Append("\\\">");

            foreach (var child in Children)
            {
                /*if (child is string)
                {
                    sb.Append(child);
                }
                else
                {*/
                    var view = child as WView;
                    view.Render(sb);
                //}
            }
            sb.Append("</w-grid>");
        }

        private static RowDefinitionCollection ParseRowDefinitions(string input)
        {
            var rows = new RowDefinitionCollection();

            foreach (var gl in ParseGridLengths(input))
                rows.Add(new RowDefinition { Height = gl });

            return rows;
        }

        private static ColumnDefinitionCollection ParseColumnDefinitions(string input)
        {
            var cols = new ColumnDefinitionCollection();

            foreach (var gl in ParseGridLengths(input))
                cols.Add(new ColumnDefinition { Width = gl });

            return cols;
        }

        private static GridLength[] ParseGridLengths(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<GridLength>();

            //comma or semicolon
            var parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new GridLength[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                result[i] = ParseSingle(parts[i].Trim());

            return result;
        }

        private static GridLength ParseSingle(string token)
        {
            if (token.Length == 0)
                throw new FormatException("Empty grid length token.");

            //auto
            if (token.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return GridLength.Auto;

            //star
            if (token.EndsWith("*", StringComparison.Ordinal))
            {
                var numberPart = token[..^1].Trim(); // remove '*'
                if (numberPart.Length == 0)
                    return new GridLength(1, GridUnitType.Star);

                if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var star))
                    throw new FormatException($"Invalid star grid length: '{token}'");

                return new GridLength(star, GridUnitType.Star);
            }

            //absolute
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var abs))
                throw new FormatException($"Invalid absolute grid length: '{token}'");

            return new GridLength(abs, GridUnitType.Absolute);
        }
    }

    public interface IWLayoutSettings
    {
        internal void AssignParent(WView parent);
    }

    public class WGridLayoutSettings : IWLayoutSettings
    {
        void IWLayoutSettings.AssignParent(WView parent)
        {
            _parent = parent;
        }

        private WView _parent;

        private int _row = 0;
        private int _column = 0;
        private int _rowSpan = 1;
        private int _colSpan = 1;

        public int Row { get => _row; set { _row = value; _parent?.MarkUpdated(); } }
        public int Column { get => _column; set { _column = value; _parent?.MarkUpdated(); } }
        public int RowSpan { get => _rowSpan; set { _rowSpan = value; _parent?.MarkUpdated(); } }
        public int ColumnSpan { get => _colSpan; set { _colSpan = value; _parent?.MarkUpdated(); } }
    }
}
