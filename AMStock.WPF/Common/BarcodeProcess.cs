using System;
using System.Drawing;
using BarcodeLib;

namespace AMStock.WPF
{
    class BarcodeProcess
    {
        Barcode b = new Barcode();
        
        public Image GetBarcode(string dataToBeEncoded, int width, int height, bool includeLabel) 
        {
            Image encodeData=null;

            const AlignmentPositions align = AlignmentPositions.CENTER;
            const TYPE type = TYPE.CODE128A;
            
            try
            {
                    b.IncludeLabel = includeLabel;
                    b.Alignment = align;

                    var rotate = Enum.GetNames(typeof(RotateFlipType))[1];
                    b.RotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), rotate, true);
                    b.LabelPosition = LabelPositions.BOTTOMCENTER;
                    
                    //===== Encoding performed here =====
                    encodeData = b.Encode(type, dataToBeEncoded.Trim(), Color.Black, Color.White, width, height);
                    //===================================
                   
            }//try
            catch
            {
                //MessageBox.Show(ex.Message);
            }//catch

            return encodeData;
        }
    }
}
