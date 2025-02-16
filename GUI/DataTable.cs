using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompSci_NEA.GUI
{
    public class DataTable
    {
        //display and layout
        private Vector2 position;
        private SpriteFont font;
        private float cellHeight;
        private float cellPadding;
        private List<string[]> tableRows;
        private string[] colHeaders;
        private List<float> colWidths;
        private Color headerColour;
        private Color rowColour;
        private Color backgroundColour;
        private float uiScale;
        private Texture2D basicTexture;
        //changing pages stuff
        private int currentPage;
        private int rowsPerPage;
        private int totalPages;
        private Button previousButton;
        private Button nextButton;
        private Text pageIndicatorText;
        //selection
        private int? selectedRow;
        private int? selectedCol;
        private MouseState prevMouseState;

        public bool IgnoreMouseClicks { get; set; }

        public DataTable(GraphicsDevice graphicsDevice, SpriteFont font, Vector2 position, string[] colHeaders, List<string[]> tableRows, float uiScale = 1.4f)
        {
            this.font = font;
            this.position = position;
            this.colHeaders = colHeaders;
            this.tableRows = tableRows;
            this.cellHeight = font.LineSpacing + 8;
            this.cellPadding = 5f;
            this.headerColour = Color.Yellow;
            this.rowColour = Color.White;
            this.backgroundColour = new Color(0, 0, 0, 150);
            this.uiScale = uiScale;

            basicTexture = new Texture2D(graphicsDevice, 1, 1);
            basicTexture.SetData(new[] { Color.White });

            //calculates dimensions stuff
            colWidths = new List<float>();
            foreach (var header in colHeaders)
            {
                float width = font.MeasureString(header).X + cellPadding * 2;
                colWidths.Add(width);
            }

            foreach (var row in tableRows)
            {
                for (int i = 0; i < row.Length && i < colWidths.Count; i++)
                {
                    float cellWidth = font.MeasureString(row[i]).X + cellPadding * 2;
                    if (cellWidth > colWidths[i])
                        colWidths[i] = cellWidth;
                }
            }

            rowsPerPage = 8;
            currentPage = 0;
            totalPages = (int)Math.Ceiling((float)tableRows.Count / rowsPerPage);

            float scaledTotalWidth = colWidths.Sum() * uiScale;
            float scaledCellHeight = cellHeight * uiScale;
            float pageNavY = position.Y + scaledCellHeight * (rowsPerPage + 1) + 10;

            previousButton = new Button(graphicsDevice, font, "Prev", new Vector2(position.X, pageNavY), 100, 40, 1.0f, new Vector2(15, 14));
            nextButton = new Button(graphicsDevice, font, "Next", new Vector2(position.X + scaledTotalWidth - 100, pageNavY), 100, 40, 1.0f, new Vector2(15, 14));
            previousButton.OnClickAction = () => { if (currentPage > 0) currentPage--; };
            nextButton.OnClickAction = () => { if (currentPage < totalPages - 1) currentPage++; };

            pageIndicatorText = new Text(font, "", new Vector2(position.X + scaledTotalWidth / 2 - 100, pageNavY + 15), Color.White, uiScale * 0.8f);
            UpdatePageIndicator();

            prevMouseState = Mouse.GetState();
        }

        public void Update()
        {
            previousButton.Update();
            nextButton.Update();

            MouseState currentMouseState = Mouse.GetState();
            if (!IgnoreMouseClicks && prevMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.LeftButton == ButtonState.Pressed)
            {
                HandleCellClick(currentMouseState);
            }
            prevMouseState = currentMouseState;

            totalPages = (int)Math.Ceiling((float)tableRows.Count / rowsPerPage);
            if (currentPage >= totalPages) currentPage = totalPages - 1;
            if (currentPage < 0) currentPage = 0;
            UpdatePageIndicator();
        }

        private void HandleCellClick(MouseState mouseState)
        {
            float scaledTotalWidth = colWidths.Sum() * uiScale;
            float scaledCellHeight = cellHeight * uiScale;
            float dataStartY = position.Y + scaledCellHeight;
            float dataEndY = position.Y + scaledCellHeight * (rowsPerPage + 1);

            //code which determines cell clickked
            if (mouseState.X >= position.X && mouseState.X <= position.X + scaledTotalWidth &&
                mouseState.Y >= dataStartY && mouseState.Y <= dataEndY)
            {
                float relativeY = mouseState.Y - dataStartY;
                int rowInPage = (int)(relativeY / scaledCellHeight);
                int globalRowIndex = currentPage * rowsPerPage + rowInPage;

                float relativeX = mouseState.X - position.X;
                int colIndex = 0;
                float cumWidth = 0;
                List<float> scaledColumnWidths = colWidths.Select(width => width * uiScale).ToList();
                for (int i = 0; i < scaledColumnWidths.Count; i++)
                {
                    cumWidth += scaledColumnWidths[i];
                    if (relativeX < cumWidth)
                    {
                        colIndex = i;
                        break;
                    }
                }

                if (globalRowIndex < tableRows.Count && colIndex < colHeaders.Length)
                {
                    selectedRow = globalRowIndex;
                    selectedCol = colIndex;
                }
                else
                {
                    ClearSelectedCell();
                }
            }
            else
            {
                ClearSelectedCell();
            }
        }

        private void UpdatePageIndicator()
        {
            pageIndicatorText.UpdateContent($"Page {currentPage + 1} / {totalPages}");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float scaledTotalWidth = colWidths.Sum() * uiScale;
            float scaledCellHeight = cellHeight * uiScale;
            float tableHeight = scaledCellHeight * (rowsPerPage + 1);

            spriteBatch.Draw(basicTexture, new Rectangle((int)position.X, (int)position.Y, (int)scaledTotalWidth, (int)tableHeight), backgroundColour);

            //col headers drawing
            Vector2 currentPos = position;
            for (int i = 0; i < colHeaders.Length; i++)
            {
                string headerText = colHeaders[i];
                Vector2 headerSize = font.MeasureString(headerText) * uiScale;
                Vector2 headerPos = new Vector2(currentPos.X + cellPadding * uiScale, currentPos.Y + (scaledCellHeight - headerSize.Y) / 2);
                spriteBatch.DrawString(font, headerText, headerPos, headerColour, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);
                currentPos.X += (colWidths[i] * uiScale);
            }

            currentPos = new Vector2(position.X, position.Y + scaledCellHeight);
            Color selectedRowColour = new Color(0, 0, 255, 80);
            Color selectedCellColour = new Color(255, 0, 0, 120);

            int startIndex = currentPage * rowsPerPage;
            int endIndex = Math.Min(startIndex + rowsPerPage, tableRows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                //highlight row which contains the selected cell
                //TODO make this bring up other tables, maybe in scene class though
                if (selectedRow.HasValue && selectedRow.Value == i)
                    spriteBatch.Draw(basicTexture, new Rectangle((int)position.X, (int)currentPos.Y, (int)scaledTotalWidth, (int)scaledCellHeight), selectedRowColour);

                string[] row = tableRows[i];
                Vector2 cellPos = currentPos;
                for (int j = 0; j < row.Length && j < colWidths.Count; j++)
                {
                    string cellText = row[j];
                    Vector2 textSize = font.MeasureString(cellText) * uiScale;
                    Vector2 textPos = new Vector2(cellPos.X + cellPadding * uiScale, cellPos.Y + (scaledCellHeight - textSize.Y) / 2);
                    spriteBatch.DrawString(font, cellText, textPos, rowColour, 0f, Vector2.Zero, uiScale, SpriteEffects.None, 0f);

                    //Red boarder around selected cell
                    if (selectedRow.HasValue && selectedCol.HasValue &&
                        selectedRow.Value == i && selectedCol.Value == j)
                    {
                        Rectangle cellRect = new Rectangle((int)cellPos.X, (int)cellPos.Y, (int)(colWidths[j] * uiScale), (int)scaledCellHeight);
                        int borderThickness = 2;
                        spriteBatch.Draw(basicTexture, new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, borderThickness), selectedCellColour);
                        spriteBatch.Draw(basicTexture, new Rectangle(cellRect.X, cellRect.Y + cellRect.Height - borderThickness, cellRect.Width, borderThickness), selectedCellColour);
                        spriteBatch.Draw(basicTexture, new Rectangle(cellRect.X, cellRect.Y, borderThickness, cellRect.Height), selectedCellColour);
                        spriteBatch.Draw(basicTexture, new Rectangle(cellRect.X + cellRect.Width - borderThickness, cellRect.Y, borderThickness, cellRect.Height), selectedCellColour);
                    }
                    cellPos.X += (colWidths[j] * uiScale);
                }
                currentPos.Y += scaledCellHeight;
            }

            previousButton.Draw(spriteBatch);
            nextButton.Draw(spriteBatch);
            pageIndicatorText.Draw(spriteBatch);
        }

        public void SetCellValue(int globalRowIndex, int columnIndex, string newValue)
        {
            if (globalRowIndex >= 0 && globalRowIndex < tableRows.Count && columnIndex >= 0 && columnIndex < colHeaders.Length)
                tableRows[globalRowIndex][columnIndex] = newValue;
        }

        public void SetSelectedCellValue(string newValue)
        {
            if (selectedRow.HasValue && selectedCol.HasValue)
            {
                //cannot change user id
                if (selectedCol.Value == 0)
                    return;

                if (selectedCol.Value == 1)
                {
                    if (newValue.Length <= 3)
                        return;
                }
                else if (selectedCol.Value == 2)
                {
                    if (!(newValue == "0" || newValue == "1" ||
                          newValue.ToLower() == "true" || newValue.ToLower() == "false"))
                        return;
                    newValue = (newValue.ToLower() == "true" || newValue == "1") ? "1" : "0";
                }
                else if (selectedCol.Value == 3)
                {
                    if (!int.TryParse(newValue, out int coinValue))
                        return;
                }

                SetCellValue(selectedRow.Value, selectedCol.Value, newValue);

                string userIdStr = tableRows[selectedRow.Value][0];
                if (int.TryParse(userIdStr, out int userId))
                {
                    string columnName = "";
                    switch (selectedCol.Value)
                    {
                        case 1:
                            columnName = "username";
                            break;
                        case 2:
                            columnName = "admin";
                            break;
                        case 3:
                            columnName = "coins";
                            break;
                        default:
                            return;
                    }
                    Database.DbFunctions dbFunctions = new Database.DbFunctions();
                    dbFunctions.UpdateUserData(userId, columnName, newValue);
                }
            }
        }

        public (int row, int col)? GetSelectedCell()
        {
            if (selectedRow.HasValue && selectedCol.HasValue)
                return (selectedRow.Value, selectedCol.Value);
            return null;
        }

        public string GetSelectedCellValue()
        {
            if (!selectedRow.HasValue || !selectedCol.HasValue)
                return string.Empty;
            return tableRows[selectedRow.Value][selectedCol.Value];
        }

        public void ClearSelectedCell() //unselect cell
        {
            selectedRow = null;
            selectedCol = null;
        }
    }
}
