using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zenject.EditorWindowTemplate
{
    public sealed class TableDrawer<T>
    {
        private readonly Styles styles;
        private readonly Table<T> table;
        private readonly EditorWindow window;
        private readonly EditorTemplateView<T> view;

        private readonly List<T> list = new List<T>();
        private bool isListDirty;

        private int controlID;
        private float scrollPosition;
        private Type selectedType;
        
        private int sortColumn;
        private bool sortDescending;
        private string searchFilter = "";
        private string actualFilter = "";
        
        internal TableDrawer(EditorWindow window, EditorTemplateView<T> view, Table<T> table)
        {
            this.window = window;
            this.view = view;
            this.table = table;
            styles = new Styles();
        }
        
        public void Tick()
        {
            if (isListDirty)
            {
                isListDirty = false;
                list.Clear();
                view.PopulateList(list);
            }

            InPlaceStableSort<T>.Sort(list, CompareTo);
        }

        public void Draw()
        {
            styles.Check();
            controlID = GUIUtility.GetControlID(FocusType.Passive);
            
            DrawFilter();
            DrawScrollRect();
            HandleEvents();
        }

        public void MarkListAsDirty()
        {
            isListDirty = true;
        }

        public string GetFilter()
        {
            return actualFilter;
        }

        private void DrawFilter()
        {
            GUI.Label(new Rect(0, 0, styles.filterPaddingLeft, styles.filterHeight), "Filter:", styles.filterTextStyle);
            string temp = GUI.TextField(new Rect(styles.filterPaddingLeft, styles.filterPaddingTop, styles.filterWidth, styles.filterInputHeight), searchFilter, 999);

            if (temp != searchFilter)
            {
                searchFilter = temp;
                actualFilter = searchFilter.Trim().ToLowerInvariant();
                isListDirty = true;
            }
        }

        private void DrawScrollRect()
        {
            var windowBounds = new Rect(0, 0, window.position.width, window.position.height);
            var scrollbarSize = new Vector2(GUI.skin.horizontalScrollbar.CalcSize(GUIContent.none).y, GUI.skin.verticalScrollbar.CalcSize(GUIContent.none).x);
            var viewArea = new Rect(0, styles.headerTop, window.position.width - scrollbarSize.y, window.position.height - styles.headerTop);
            var contentRect = new Rect(0, 0, viewArea.width, list.Count() * styles.rowHeight);
            var vScrRect = new Rect(windowBounds.x + viewArea.width, styles.headerTop, scrollbarSize.y, viewArea.height);

            scrollPosition = GUI.VerticalScrollbar(vScrRect, scrollPosition, viewArea.height, 0, contentRect.height);
            DrawColumnHeaders(viewArea.width);

            GUI.BeginGroup(viewArea);
            {
                contentRect.y = -scrollPosition;

                GUI.BeginGroup(contentRect);
                {
                    DrawContent();
                }
                GUI.EndGroup();
            }
            GUI.EndGroup();
        }
        
        private void DrawColumnHeaders(float width)
        {
            GUI.DrawTexture(new Rect(0, styles.filterHeight - 0.5f * styles.splitterWidth, width, styles.splitterWidth), styles.lineTexture);
            GUI.DrawTexture(new Rect(0, styles.headerTop - 0.5f * styles.splitterWidth, width, styles.splitterWidth), styles.lineTexture);

            var columnPos = 0.0f;
            int count = GetColumnCount();

            for (var i = 0; i < count; i++)
            {
                float columnWidth = GetColumnWidth(i);
                DrawColumnHeader(i, columnPos, columnWidth);
                columnPos += columnWidth;
            }
        }

        private void DrawColumnHeader(int index, float position, float width)
        {
            float columnHeight = styles.headerHeight + list.Count() * styles.rowHeight;
            var headerBounds = new Rect(position + 0.5f * styles.splitterWidth, styles.filterHeight, width - styles.splitterWidth, styles.headerHeight);
                        
            int splitterCount = table.columns.Length;

            if (table.options == null || table.options.Length == 0)
                splitterCount--;
            
            if (index < splitterCount)
                GUI.DrawTexture(new Rect(position + width - styles.splitterWidth * 0.5f, styles.filterHeight, styles.splitterWidth, columnHeight), styles.lineTexture);
            
            if (index < table.columns.Length)
                DrawColumnHeader(index, headerBounds, table.columns[index].title, true);
            else
                DrawColumnHeader(index, headerBounds, "Options", false);
        }

        private void DrawColumn(int index, float position, float width)
        {
            float columnHeight = styles.headerHeight + list.Count() * styles.rowHeight;
            var columnBounds = new Rect(position + 0.5f * styles.splitterWidth, 0, width - styles.splitterWidth, columnHeight);
            
            int splitterCount = table.columns.Length;

            if (table.options == null || table.options.Length == 0)
                splitterCount--;
            
            if (index < splitterCount)
                GUI.DrawTexture(new Rect(position + width - styles.splitterWidth * 0.5f, 0, styles.splitterWidth, columnHeight), styles.lineTexture);

            GUI.BeginGroup(columnBounds);
            {
                for (var i = 0; i < list.Count; i++)
                {
                    T pool = list[i];

                    var cellBounds = new Rect(0, styles.rowHeight * i, columnBounds.width, styles.rowHeight);
                    DrawColumnContents(index, cellBounds, pool);
                }
            }
            GUI.EndGroup();
        }

        private void DrawRowBackgrounds()
        {
            Vector2 mousePositionInContent = Event.current.mousePosition;

            for (var i = 0; i < list.Count; i++)
            {
                T pool = list[i];
                Rect rowRect = GetRowRect(i);

                Texture2D background;

                if (pool.GetType() == selectedType)
                {
                    background = styles.rowBackgroundSelected;
                }
                else
                {
                    if (rowRect.Contains(mousePositionInContent))
                        background = styles.rowBackgroundHighlighted;
                    else if (i % 2 == 0)
                        background = styles.rowBackground1;
                    else
                        background = styles.rowBackground2;
                }

                GUI.DrawTexture(rowRect, background);
            }
        }
        
        private void DrawContent()
        {
            DrawRowBackgrounds();

            int count = GetColumnCount();
            var columnPos = 0.0f;

            for (var i = 0; i < count; i++)
            {
                float columnWidth = GetColumnWidth(i);
                DrawColumn(i, columnPos, columnWidth);
                columnPos += columnWidth;
            }
        }
        
        private void DrawColumnContents(int index, Rect bounds, T t)
        {
            if (index < table.columns.Length)
            {
                GUIStyle style = index == 0 ? styles.contentTextLeft : styles.contentTextCenter;
                GUI.Label(bounds, table.columns[index].action.Invoke(t).ToString(), style);
            }
            else if (table.options != null && table.options.Length > 0)
            {
                bounds.x += 1;
                bounds.y += 1;
                bounds.width -= 2;
                bounds.height -= 2;

                bounds.width /= table.options.Length;

                for (var i = 0; i < table.options.Length; i++)
                {
                    if (GUI.Button(bounds, table.options[i].title))
                        table.options[i].action.Invoke(t);
                    
                    bounds.x += bounds.width;
                }
            }
        }

        private void DrawColumnHeader(int index, Rect bounds, string text, bool sortable)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (sortColumn == index)
            {
                Vector2 offset = styles.triangleOffset;
                Texture2D image = sortDescending ? styles.triangleDown : styles.triangleUp;
                GUI.DrawTexture(new Rect(bounds.x + offset.x, bounds.y + offset.y, image.width, image.height), image);
            }

            GUIStyle style = index == 0 ? styles.headerTextLeft : styles.headerTextCenter;

            if (GUI.Button(bounds, text, style) && sortable)
            {
                if (sortColumn == index)
                    sortDescending = !sortDescending;
                else
                    sortColumn = index;
            }
        }
        
        private void HandleEvents()
        {
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.ScrollWheel:
                {
                    scrollPosition = Mathf.Clamp(scrollPosition + Event.current.delta.y * styles.scrollSpeed, 0, window.position.height);
                    break;
                }
                case EventType.MouseDown:
                {
                    selectedType = TryGetPoolTypeUnderMouse();
                    break;
                }
            }
        }
        
        private Type TryGetPoolTypeUnderMouse()
        {
            Vector2 mousePositionInContent = Event.current.mousePosition + Vector2.up * scrollPosition;

            for (var i = 0; i < list.Count; i++)
            {
                T pool = list[i];
                Rect rowRect = GetRowRect(i);
                rowRect.y += styles.headerTop;

                if (rowRect.Contains(mousePositionInContent))
                    return pool.GetType();
            }

            return null;
        }

        private Rect GetRowRect(int index)
        {
            return new Rect(0, index * styles.rowHeight, window.position.width, styles.rowHeight);
        }
        
        private int GetColumnCount()
        {
            int count = table.columns.Length;

            if (table.options != null && table.options.Length > 0)
                count++;

            return count;
        }
        
        private float GetColumnWidth(int index)
        {
            int count = GetColumnCount();
            
            if (index == 0)
                return window.position.width - (count - 1) * styles.normalColumnWidth - 14.0f;

            return styles.normalColumnWidth;
        }

        private int CompareTo(T left, T right)
        {
            if (sortDescending)
            {
                T temp = right;
                right = left;
                left = temp;
            }

            IComparable leftValue = table.columns[sortColumn].action.Invoke(left);
            IComparable rightValue = table.columns[sortColumn].action.Invoke(right);
            return leftValue.CompareTo(rightValue);
        }
    }
}