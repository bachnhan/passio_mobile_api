using HmsService.Models;
using HmsService.Models.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Wisky.SkyAdmin.Manage.Printer
{
    public class PDFPrinter
    {
        // Utils4Printer contains common functions
        private Utils4Printer utils4Printer = new Utils4Printer();

        // Set up the fonts to be used in the document 
        private static BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @Properties.Resources.FONT_PATH, BaseFont.IDENTITY_H, true);
        private Font _largeBoldFont = new Font(bf, 10, Font.BOLD, BaseColor.BLACK);
        private Font _largeBoldItalicFont = new Font(bf, 10, Font.BOLDITALIC, BaseColor.BLACK);
        private Font _standardBoldFont = new Font(bf, 8, Font.BOLD, BaseColor.BLACK);
        private Font _standardBoldItalicFont = new Font(bf, 8, Font.BOLDITALIC, BaseColor.BLACK);
        private Font _smallFont = new Font(bf, 6, Font.NORMAL, BaseColor.BLACK);
        private Font _smallBoldFont = new Font(bf, 6, Font.BOLD, BaseColor.BLACK);
        private Font _smallItalicFont = new Font(bf, 6, Font.ITALIC, BaseColor.BLACK);
        private Font _smallBoldItalicFont = new Font(bf, 6, Font.BOLDITALIC, BaseColor.BLACK);

        /// <summary>
        /// Export PDF document
        /// </summary>
        /// <param name="entity">VATOrder object</param>
        public byte[] PrintPDF(VATOrder entity)
        {
            MemoryStream ms = new MemoryStream();
            Document doc = new Document();
            Phrase phrase = null;
            PdfPCell cell = null;
            PdfPTable table = null;

            try
            {
                // Initialize the PDF document 
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);

                // Set margins and page size for the document 
                doc.SetMargins(50, 50, 50, 50);
                doc.SetPageSize(new Rectangle(PageSize.A5));

                // Open the document for writing content 
                doc.Open();

                // 1st copy 
                CreateDocument(doc, table, phrase, cell, writer, entity, 1);

                // 2nd copy 
                CreateDocument(doc, table, phrase, cell, writer, entity, 2);

                // Close the document
                doc.Close();

                return ms.ToArray();
            }
            catch (DocumentException dex)
            {
                // Handle iTextSharp errors 
                throw dex;
            }
            finally
            {
                // Clean up stream
                ms.Close();
            }
        }

        /// <summary>
        /// Create the document.  
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="table">Table which to add to the document</param>
        /// <param name="phrase">Phrase which to add to the cell</param>
        /// <param name="cell">Cell which to add to the table</param>
        /// <param name="writer">PDF writer</param>  
        /// <param name="entity">VATOrder object</param>
        /// <param name="copy">1st/2nd copy</param>
        private void CreateDocument(Document doc, PdfPTable table, Phrase phrase, PdfPCell cell, PdfWriter writer, VATOrder entity, int copy)
        {
            // Add page header to the document 
            AddPageHeader(doc, table, phrase, cell, entity);

            // Line seperator
            doc.Add(new LineSeparator(1F, 100.0F, BaseColor.BLACK, Element.ALIGN_CENTER, 1));

            // Add page content to the document 
            AddPageContent(doc, table, phrase, cell, entity, copy);

            // Add page footer to the document 
            AddPageFooter(doc, table, phrase, cell, entity);

            // Add border to the document 
            AddPageBorder(doc, writer);
        }

        /// <summary>
        /// Add the page header to the document.  
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="table">Table which to add to the document</param>
        /// <param name="phrase">Phrase which to add to the cell</param>
        /// <param name="cell">Cell which to add to the table</param>
        /// <param name="entity">VATOrder object</param>
        private void AddPageHeader(Document doc, PdfPTable table, Phrase phrase, PdfPCell cell, VATOrder entity)
        {
            //Page header table
            table = new PdfPTable(2);
            table.TotalWidth = 310f;
            table.LockedWidth = true;
            float tblWidth = PageSize.A5.Rotate().Width - 50;
            table.SetWidths(new float[] { tblWidth*2/5,
                                         tblWidth*3/5
                                        });
            // Add a logo
            Image logoImage = Image.GetInstance(HttpContext.Current.Server.MapPath(Properties.Resources.HEADER_LOGO));
            logoImage.ScaleAbsoluteWidth(110f);
            logoImage.ScaleAbsoluteHeight(40f);
            logoImage.Alignment = Element.ALIGN_CENTER;
            cell = ImageCell(logoImage, PdfPCell.ALIGN_CENTER, PdfPCell.ALIGN_CENTER);
            cell.BorderColor = BaseColor.WHITE;
            table.AddCell(cell);


            // Write table content.  Add table to cell of page header table.
            PdfPTable tmpTable = new PdfPTable(1);
            tmpTable.SetWidths(new float[] { 1 });

            // Company name
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.HEADER_COMP_VN, null, null, 1, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, null, null, Properties.Resources.HEADER_COMP_EN, 1, false, true);
            // Address
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.ADDRESS_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.ADDRESS_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, Properties.Resources.WHITE_SPACE + Properties.Resources.HEADER_ADDRESS, 3, false, true);
            // Tel
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.TEL_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.TEL_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, Properties.Resources.WHITE_SPACE + Properties.Resources.HEADER_TEL, 3, false, true);
            // VAT code
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.VAT_CODE_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.VAT_CODE_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, Properties.Resources.WHITE_SPACE + Properties.Resources.HEADER_VAT_CODE, 4, false, true);

            cell = TableCell(tmpTable, PdfPCell.ALIGN_LEFT);
            cell.PaddingTop = 5f;
            cell.BorderColor = BaseColor.WHITE;
            table.AddCell(cell);
            table.SpacingAfter = 5f;

            doc.Add(table);
        }

        /// <summary>
        /// Add the content to the document.
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="table">Table which to add to the document</param>
        /// <param name="phrase">Phrase which to add to the cell</param>
        /// <param name="cell">Cell which to add to the table</param>
        /// <param name="entity">VATOrder object</param>
        /// <param name="copy">1st/2nd copy</param>
        private void AddPageContent(Document doc, PdfPTable table, Phrase phrase, PdfPCell cell, VATOrder entity, int copy)
        {
            #region Header
            //Content header table
            table = new PdfPTable(3);
            table.TotalWidth = 310f;
            table.LockedWidth = true;
            float tblWidth = PageSize.A5.Rotate().Width - 50;
            table.SetWidths(new float[] { tblWidth*4/17,
                                         tblWidth*9/17,
                                          tblWidth*4/17
                                        });

            // Write phrase content.  Add pharse to cell of content header table. 
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.WHITE_SPACE, _smallFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
            cell.BorderColor = BaseColor.WHITE;
            table.AddCell(cell);

            // Write table content.  Add table to cell of page content table.
            PdfPTable tmpTable = new PdfPTable(1);
            tmpTable.SetWidths(new float[] { 1 });

            // VAT invoice title
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.VAT_INVOICE_VN, null, null, 5, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, null, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.VAT_INVOICE_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 5, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 0, false, true);

            // 1st copy
            if (copy == 1)
            {
                SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.FIRST_COPY_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.FIRST_COPY_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 1, false, true);
            }
            else // 2st copy
            {
                SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.SECOND_COPY_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SECOND_COPY_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 1, false, true);
            }
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 0, false, true);

            // Date time
            DateTime now = entity.CheckInDate;
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.DATE_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.DATE_EN + Properties.Resources.RIGHT_PARENTHESIS, _smallItalicFont));
            phrase.Add(new Chunk(Properties.Resources.DOUBLE_WHITE_SPACE + now.Day.ToString(), _smallFont));
            phrase.Add(new Chunk(Properties.Resources.DOUBLE_WHITE_SPACE + Properties.Resources.MONTH_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.MONTH_EN + Properties.Resources.RIGHT_PARENTHESIS, _smallItalicFont));
            phrase.Add(new Chunk(Properties.Resources.DOUBLE_WHITE_SPACE + now.Month.ToString(), _smallFont));
            phrase.Add(new Chunk(Properties.Resources.DOUBLE_WHITE_SPACE + Properties.Resources.YEAR_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.YEAR_EN + Properties.Resources.RIGHT_PARENTHESIS, _smallItalicFont));
            phrase.Add(new Chunk(Properties.Resources.DOUBLE_WHITE_SPACE + now.Year.ToString(), _smallFont));

            cell = PhraseCell(phrase, PdfPCell.ALIGN_CENTER);
            cell.BorderColor = BaseColor.WHITE;
            tmpTable.AddCell(cell);

            cell = TableCell(tmpTable, PdfPCell.ALIGN_CENTER);
            cell.BorderColor = BaseColor.WHITE;
            table.AddCell(cell);

            tmpTable = new PdfPTable(1);
            tmpTable.SetWidths(new float[] { 1 });

            // Form
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.FORM_VN + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE + Properties.Resources.FORM_VALUE, null, null, 0, false, true);
            // Serial
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.SERIAL_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SERIAL_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE + Properties.Resources.SERIAL_VALUE, null, 3, false, true);
            // Invoice no.
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.NO_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.NO_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.InvoiceNo, 4, false, true);

            cell = TableCell(tmpTable, PdfPCell.ALIGN_LEFT);
            cell.BorderColor = BaseColor.WHITE;
            table.AddCell(cell);
            table.SpacingBefore = 5f;
            table.SpacingAfter = 10f;

            doc.Add(table);
            #endregion

            #region Customer info
            //Content customer information table
            table = new PdfPTable(1);
            table.TotalWidth = 310f;
            table.LockedWidth = true;
            table.SetWidths(new float[] { 1 });

            tmpTable = new PdfPTable(2);
            tmpTable.SetWidths(new float[] {
                                         tblWidth/2,
                                         tblWidth/2
                                        });

            // Write phrase content.  Add pharse to cell of content customer information table. 
            // Customer name
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.CUSTOMER_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.CUSTOMER_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.Provider.ManagerName, 4, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 3, false, true);
            // Company name
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.COMPANY_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.COMPANY_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.Provider.ProviderName, 4, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 3, false, true);
            // Address
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.ADDRESS_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.ADDRESS_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.Provider.Address, 3, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 3, false, true);
            // VAT code
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.VAT_CODE_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.VAT_CODE_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.Provider.VATCode, 3, false, true);
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 3, false, true);
            // Payment method
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.PAYMENT_METHOD_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.PAYMENT_METHOD_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, ((int)PaymentMethod.Cash == entity.Type) ? PaymentMethod.Cash.DisplayName() : PaymentMethod.Card.DisplayName(), 3, false, true);
            // Account no.
            SetTableData(phrase, cell, tmpTable, PdfPCell.ALIGN_LEFT, Properties.Resources.ACCOUNT_NO_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.ACCOUNT_NO_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, entity.Provider.AccountNo, 3, false, true);

            cell = TableCell(tmpTable, PdfPCell.ALIGN_LEFT);
            cell.Padding = 1f;
            cell.BorderColor = BaseColor.BLACK;
            table.AddCell(cell);
            table.SpacingAfter = 5f;

            doc.Add(table);
            #endregion

            #region Order detail
            //Content order detail table
            table = new PdfPTable(6);
            table.TotalWidth = 310f;
            table.LockedWidth = true;
            table.SetWidths(new float[] {
                                         tblWidth/15,
                                         tblWidth/3,
                                         tblWidth/10,
                                         tblWidth*2/17,
                                         tblWidth/5,
                                         tblWidth/5
                                        });

            // Write phrase content.  Add pharse to cell of content order detail table.
            // Set order detail table header 
            // Order no.
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.ORDER_NO_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.NO_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);
            // Description
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.DESCRIPTION_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.DESCRIPTION_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);
            // Unit
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.UNIT_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.UNIT_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);
            // Quantity
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.QUANTITY_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.QUANTITY_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);
            // Unit price
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.UNIT_PRICE_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.UNIT_PRICE_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);
            // Amount
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.AMOUNT_VN + Environment.NewLine, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.AMOUNT_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 0, true, true);

            // Set order detail table body
            // First row
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.ONE, null, null, 0, true, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.TWO, null, null, 0, true, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.THREE, null, null, 0, true, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.FOUR, null, null, 0, true, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.FIVE, null, null, 0, true, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.SIX + Properties.Resources.WHITE_SPACE + Properties.Resources.EQUAL + Properties.Resources.WHITE_SPACE + Properties.Resources.FOUR + Properties.Resources.WHITE_SPACE + Properties.Resources.MULTIPLY + Properties.Resources.WHITE_SPACE + Properties.Resources.FIVE, null, null, 0, true, true);

            // Counted variable
            int i = 1;

            if (entity.VATOrderDetail != null)
            {
                dynamic arr = JObject.Parse(entity.VATOrderDetail);
                foreach (JProperty o in arr)
                {
                    string name = o.Name;

                    // Ignore first item
                    if (!name.Equals("0"))
                    {
                        var value = o.Value.ToList();

                        // details of first 12 products
                        if (i < 13)
                        {
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, i.ToString(), null, null, 0, true, false);
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_LEFT, value[0].ToString(), null, null, 0, true, false);
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, value[3].ToString(), null, null, 0, true, false);
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, utils4Printer.FormatNumber(double.Parse(value[2].ToString())), null, null, 0, true, false);
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_RIGHT, utils4Printer.FormatNumber(double.Parse(value[1].ToString())), null, null, 0, true, false);
                            SetTableData(phrase, cell, table, PdfPCell.ALIGN_RIGHT, utils4Printer.FormatNumber((double.Parse(value[2].ToString()) * double.Parse(value[1].ToString()))), null, null, 0, true, false);
                        }
                        i++;
                    }

                }
            }

            // filling the table in case there is less than 12 products
            if (i < 13)
            {
                for (int j = i; j < 13; j++)
                {
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_LEFT, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_RIGHT, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                    SetTableData(phrase, cell, table, PdfPCell.ALIGN_RIGHT, Properties.Resources.WHITE_SPACE, null, null, 0, true, false);
                }
            }
            #endregion

            #region Footer
            // Set order detail table footer
            // Total amount
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.TOTAL_AMOUNT_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.TOTAL_AMOUNT_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, _smallItalicFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            cell.Colspan = 5;
            table.AddCell(cell);

            phrase = new Phrase();
            phrase.Add(new Chunk(utils4Printer.FormatNumber(entity.Total), _smallBoldFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            table.AddCell(cell);

            // VAT rate
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.VAT_RATE_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.VAT_RATE_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, _smallItalicFont));
            phrase.Add(new Chunk(Properties.Resources.VAT, _smallBoldFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
            cell.Colspan = 2;
            cell.BorderWidthRight = 0f;
            table.AddCell(cell);

            // VAT amount
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.VAT_AMOUNT_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.VAT_AMOUNT_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, _smallItalicFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            cell.Colspan = 3;
            cell.BorderWidthLeft = 0f;
            table.AddCell(cell);

            phrase = new Phrase();
            phrase.Add(new Chunk(utils4Printer.FormatNumber(entity.VATAmount), _smallBoldFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            table.AddCell(cell);

            // Grand total
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.GRAND_TOTAL_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.GRAND_TOTAL_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON, _smallItalicFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            cell.Colspan = 5;
            table.AddCell(cell);

            phrase = new Phrase();
            phrase.Add(new Chunk(utils4Printer.FormatNumber((entity.Total + entity.VATAmount)), _smallBoldFont));
            cell = PhraseCell(phrase, PdfPCell.ALIGN_RIGHT);
            table.AddCell(cell);

            // In word
            phrase = new Phrase();
            phrase.Add(new Chunk(Properties.Resources.IN_WORD_VN + Properties.Resources.WHITE_SPACE, _smallFont));
            phrase.Add(new Chunk(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.IN_WORD_EN + Properties.Resources.RIGHT_PARENTHESIS + Properties.Resources.COLON + Properties.Resources.WHITE_SPACE, _smallItalicFont));
            string inWord = utils4Printer.ConvertNum2Words(entity.Total + entity.VATAmount) + Properties.Resources.CURRENCY_VN;
            phrase.Add(new Chunk(inWord, _smallFont));
            if (inWord.Length > 65)
            {
                phrase.Add(new Chunk(Environment.NewLine + Properties.Resources.WHITE_SPACE, _smallFont));
            }
            else
            {
                phrase.Add(new Chunk(Environment.NewLine + Environment.NewLine + Properties.Resources.WHITE_SPACE, _smallFont));
            }
            cell = PhraseCell(phrase, PdfPCell.ALIGN_LEFT);
            cell.Colspan = 6;
            table.AddCell(cell);
            table.SpacingAfter = 5f;

            doc.Add(table);
            #endregion
        }

        /// <summary>
        /// Add the page footer to the document.
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="table">Table which to add to the document</param>
        /// <param name="phrase">Phrase which to add to the cell</param>
        /// <param name="cell">Cell which to add to the table</param>
        /// <param name="entity">VATOrder object</param>
        private void AddPageFooter(Document doc, PdfPTable table, Phrase phrase, PdfPCell cell, VATOrder entity)
        {
            //Page footer table
            table = new PdfPTable(3);
            table.TotalWidth = 310f;
            table.LockedWidth = true;
            float tblWidth = PageSize.A5.Rotate().Width - 50;
            table.SetWidths(new float[] {
                                         tblWidth/3,
                                         tblWidth/3,
                                         tblWidth/3
                                        });

            // Write phrase content.  Add pharse to cell of content footer table. 
            // Buyer, seller and director
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.BUYER_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.BUYER_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.SELLER_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SELLER_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, Properties.Resources.DIRECTOR_VN + Properties.Resources.WHITE_SPACE, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.DIRECTOR_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);
            //Sign, stamp and full name
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.SIGN_AND_FULLNAME_VN, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.SIGN_AND_FULLNAME_VN, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.SIGN_STAMP_AND_FULLNAME_VN, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SIGN_AND_FULLNAME_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SIGN_AND_FULLNAME_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);
            SetTableData(phrase, cell, table, PdfPCell.ALIGN_CENTER, null, Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.SIGN_STAMP_AND_FULLNAME_EN + Properties.Resources.RIGHT_PARENTHESIS, null, 3, false, true);

            table.SpacingAfter = 50f;
            doc.Add(table);

            Paragraph paragraph = new Paragraph();
            paragraph.Alignment = PdfPCell.ALIGN_CENTER;
            paragraph.Font = _smallItalicFont;
            paragraph.Add(Properties.Resources.LEFT_PARENTHESIS + Properties.Resources.NOTE_VN + Properties.Resources.RIGHT_PARENTHESIS);

            doc.Add(paragraph);
        }

        /// <summary>
        /// Assign color, alignment, padding information for cell (cell contains phrase)
        /// </summary>
        /// <param name="phrase">Pharse to which to add the cell.</param>
        /// <param name="align">Alignment of the cell.</param>
        private PdfPCell PhraseCell(Phrase phrase, int align)
        {
            PdfPCell cell = new PdfPCell(phrase);
            cell.HorizontalAlignment = align;
            cell.SetLeading(6f, 0f);
            cell.UseVariableBorders = true;
            return cell;
        }

        /// <summary>
        /// Assign color, alignment, padding information for cell (cell contains table)
        /// </summary>
        /// <param name="table">Table to which to add the cell.</param>
        /// <param name="align">Alignment of the cell.</param>
        private PdfPCell TableCell(PdfPTable table, int align)
        {
            PdfPCell cell = new PdfPCell(table);
            cell.HorizontalAlignment = align;
            cell.PaddingTop = 0f;
            cell.PaddingBottom = 0f;
            cell.UseVariableBorders = true;
            return cell;
        }

        /// <summary>
        /// Assign color, alignment, padding information for cell (cell contains inmage)
        /// </summary>
        /// <param name="image">Image to which to add the cell.</param>
        /// <param name="hAlign">Horizontal alignment of the cell.</param>
        /// <param name="vAlign">Vertical alignment of the cell.</param>
        private PdfPCell ImageCell(Image image, int hAlign, int vAlign)
        {
            PdfPCell cell = new PdfPCell(image);
            cell.HorizontalAlignment = hAlign;
            cell.VerticalAlignment = vAlign;
            cell.PaddingTop = 7.5f;
            cell.PaddingBottom = 7.5f; ;
            cell.UseVariableBorders = true;
            return cell;
        }

        /// <summary>
        /// Set data for order detail table
        /// </summary>
        /// <param name="phrase">Pharse which to add to the cell</param>
        /// <param name="cell">Cell which to add to the table</param>
        /// <param name="table">Table which to add to the document</param>
        /// <param name="align">>Alignment of the cell</param>
        /// <param name="strValue_1">Value 1 which to set to the pharse</param>
        /// <param name="strValue_2">Value 2 which to set to the pharse</param>
        /// <param name="strValue_3">Value 3 which to set to the pharse</param>
        /// <param name="type">Font style of the cell.</param>
        /// <param name="isBorder">Cell has border or not</param>
        /// <param name="isTopBotBorder">Cell has bottom border or not</param>
        private void SetTableData(Phrase phrase, PdfPCell cell, PdfPTable table, int align, string strValue_1, string strValue_2, string strValue_3, int type, bool isBorder, bool isTopBotBorder)
        {
            phrase = new Phrase();

            switch (type)
            {
                case 1:
                    ChunkPhrase(phrase, strValue_1, _standardBoldFont, strValue_2, _smallBoldItalicFont, strValue_3, _standardBoldFont);
                    break;
                case 2:
                    ChunkPhrase(phrase, strValue_1, _smallFont, strValue_2, _standardBoldFont, strValue_3, _smallFont);
                    break;
                case 3:
                    ChunkPhrase(phrase, strValue_1, _smallFont, strValue_2, _smallItalicFont, strValue_3, _smallFont);
                    break;
                case 4:
                    ChunkPhrase(phrase, strValue_1, _smallFont, strValue_2, _smallItalicFont, strValue_3, _standardBoldFont);
                    break;
                case 5:
                    ChunkPhrase(phrase, strValue_1, _largeBoldFont, strValue_2, _largeBoldItalicFont, strValue_3, _largeBoldFont);
                    break;
                default:
                    ChunkPhrase(phrase, strValue_1, _smallFont, strValue_2, _smallFont, strValue_3, _smallFont);
                    break;
            }

            cell = PhraseCell(phrase, align);
            if (!isBorder)
            {
                cell.BorderColor = BaseColor.WHITE;
            }
            if (!isTopBotBorder)
            {
                cell.BorderWidthTop = 0f;
                cell.BorderWidthBottom = 0f;
            }
            table.AddCell(cell);
        }

        /// <summary>
        /// Add chunk to phrase
        /// </summary>
        /// <param name="phrase">>Pharse which to add to the cell</param>
        /// <param name="strValue_1">First string</param>
        /// <param name="font_1">First font</param>
        /// <param name="strValue_2">Second string</param>
        /// <param name="font_2">Second font</param>
        /// <param name="strValue_3">Third string</param>
        /// <param name="font_3">Third font</param>
        private void ChunkPhrase(Phrase phrase, string strValue_1, Font font_1, string strValue_2, Font font_2, string strValue_3, Font font_3)
        {
            phrase.Add(new Chunk(strValue_1, font_1));
            phrase.Add(new Chunk(strValue_2, font_2));
            phrase.Add(new Chunk(strValue_3, font_3));
        }

        /// <summary>
        /// Add border to the page
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="writer">PDF writer</param>
        private void AddPageBorder(Document doc, PdfWriter writer)
        {
            //Add border to page
            #region Adding outside border
            PdfContentByte content = writer.DirectContent;
            Rectangle rec_1 = CreateRectangle(doc, 4);
            content.Rectangle(rec_1.Left, rec_1.Bottom, rec_1.Width, rec_1.Height);

            Rectangle rec_2 = CreateRectangle(doc, 3);
            content.Rectangle(rec_2.Left, rec_2.Bottom, rec_2.Width, rec_2.Height);
            #endregion

            #region Adding inside border
            Rectangle rec_3 = CreateRectangle(doc, 0);
            content.Rectangle(rec_3.Left, rec_3.Bottom, rec_3.Width, rec_3.Height);
            #endregion

            content.SetColorStroke(BaseColor.BLACK);
            content.Stroke();
        }

        /// <summary>
        /// Create a rectangle
        /// </summary>
        /// <param name="doc">PDF document</param>
        /// <param name="val">Adjusment value</param>
        private Rectangle CreateRectangle(Document doc, int val)
        {
            Rectangle rectangle = new Rectangle(doc.PageSize);
            rectangle.Left += doc.LeftMargin - val;
            rectangle.Right -= doc.RightMargin - val;
            rectangle.Top -= doc.TopMargin - val;
            rectangle.Bottom += doc.BottomMargin - val;
            return rectangle;
        }
    }
}