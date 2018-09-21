using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering.Drawing {
  static class PolySubPix {
    public const int SHIFT = 8;          //----poly_subpixel_shif
    public const int SCALE = 1 << SHIFT; //----poly_subpixel_scale 
    public const int MASK = SCALE - 1;  //----poly_subpixel_mask 
  }
  class ScanlineRasterizer {
    enum FillingRule {
      NonZero,
      EvenOdd
    }

    //-----------------------------------------------------------------cell_aa
    // A pixel cell. There're no constructors defined and it was done ***
    // intentionally in order to avoid extra overhead when allocating an ****
    // array of cells. ***
    readonly struct CellAA {
      public readonly int x;
      public readonly int y;
      public readonly int cover;
      public readonly int area;
      public CellAA(int x, int y, int cover, int area) {
        this.x = x;
        this.y = y;
        this.cover = cover;
        this.area = area;
      }
    }


    //-----------------------------------------------------rasterizer_cells_aa
    // An internal class that implements the main rasterization algorithm.
    // Used in the rasterizer. Should not be used directly.
    sealed class CellAARasterizer {
      DrawingList<CellAA> m_cells;
      DrawingList<CellAA> m_sorted_cells;
      DrawingList<SortedY> m_sorted_y;
      //------------------
      int cCell_x;
      int cCell_y;
      int cCell_cover;
      int cCell_area;
      const int BLOCK_SHIFT = 12;
      const int BLOCK_SIZE = 1 << BLOCK_SHIFT;
      const int BLOCK_MASK = BLOCK_SIZE - 1;
      const int BLOCK_POOL = 256;
      const int BLOCK_LIMIT = BLOCK_SIZE * 1024;
      struct SortedY {
        internal int start;
        internal int num;
      }

      public CellAARasterizer() {
        m_sorted_cells = new DrawingList<CellAA>();
        m_sorted_y = new DrawingList<SortedY>();
        MinX = (0x7FFFFFFF);
        MinY = (0x7FFFFFFF);
        MaxX = (-0x7FFFFFFF);
        MaxY = (-0x7FFFFFFF);
        Sorted = false;
        ResetCurrentCell();
        this.m_cells = new DrawingList<CellAA>(BLOCK_SIZE);
      }
      void ResetCurrentCell() {
        cCell_x = 0x7FFFFFFF;
        cCell_y = 0x7FFFFFFF;
        cCell_cover = 0;
        cCell_area = 0;
      }

      public void Reset() {
        TotalCells = 0;
        ResetCurrentCell();
        Sorted = false;
        MinX = 0x7FFFFFFF;
        MinY = 0x7FFFFFFF;
        MaxX = -0x7FFFFFFF;
        MaxY = -0x7FFFFFFF;
      }


      const int DX_LIMIT = (16384 << PolySubPix.SHIFT);
      const int POLY_SUBPIXEL_SHIFT = PolySubPix.SHIFT;
      const int POLY_SUBPIXEL_MASK = PolySubPix.MASK;
      const int POLY_SUBPIXEL_SCALE = PolySubPix.SCALE;
      public void DrawLine(int x1, int y1, int x2, int y2) {
        int dx = x2 - x1;
        if (dx >= DX_LIMIT || dx <= -DX_LIMIT) {
          int cx = (x1 + x2) >> 1;
          int cy = (y1 + y2) >> 1;
          DrawLine(x1, y1, cx, cy);
          DrawLine(cx, cy, x2, y2);
        }

        int dy = y2 - y1;
        int ex1 = x1 >> POLY_SUBPIXEL_SHIFT;
        int ex2 = x2 >> POLY_SUBPIXEL_SHIFT;
        int ey1 = y1 >> POLY_SUBPIXEL_SHIFT;
        int ey2 = y2 >> POLY_SUBPIXEL_SHIFT;
        int fy1 = y1 & POLY_SUBPIXEL_MASK;
        int fy2 = y2 & POLY_SUBPIXEL_MASK;
        int x_from, x_to;
        int p, rem, mod, lift, delta, first, incr;
        if (ex1 < MinX) MinX = ex1;
        if (ex1 > MaxX) MaxX = ex1;
        if (ey1 < MinY) MinY = ey1;
        if (ey1 > MaxY) MaxY = ey1;
        if (ex2 < MinX) MinX = ex2;
        if (ex2 > MaxX) MaxX = ex2;
        if (ey2 < MinY) MinY = ey2;
        if (ey2 > MaxY) MaxY = ey2;
        //***
        AddNewCell(ex1, ey1);
        //***
        //everything is on a single horizontal line
        if (ey1 == ey2) {
          RenderHLine(ey1, x1, fy1, x2, fy2);
          return;
        }

        //Vertical line - we have to calculate start and end cells,
        //and then - the common values of the area and coverage for
        //all cells of the line. We know exactly there's only one 
        //cell, so, we don't have to call render_hline().
        incr = 1;
        if (dx == 0) {
          int ex = x1 >> POLY_SUBPIXEL_SHIFT;
          int two_fx = (x1 - (ex << POLY_SUBPIXEL_SHIFT)) << 1;
          int area;
          first = POLY_SUBPIXEL_SCALE;
          if (dy < 0) {
            first = 0;
            incr = -1;
          }

          x_from = x1;
          delta = first - fy1;
          cCell_cover += delta;
          cCell_area += two_fx * delta;
          ey1 += incr;
          //***
          AddNewCell(ex, ey1);
          //***
          delta = first + first - POLY_SUBPIXEL_SCALE;
          area = two_fx * delta;
          while (ey1 != ey2) {
            cCell_cover = delta;
            cCell_area = area;
            ey1 += incr;
            //***
            AddNewCell(ex, ey1);
            //***
          }
          delta = fy2 - POLY_SUBPIXEL_SCALE + first;
          cCell_cover += delta;
          cCell_area += two_fx * delta;
          return;
        }

        //ok, we have to render several hlines
        p = (POLY_SUBPIXEL_SCALE - fy1) * dx;
        first = POLY_SUBPIXEL_SCALE;
        if (dy < 0) {
          p = fy1 * dx;
          first = 0;
          incr = -1;
          dy = -dy;
        }

        delta = p / dy;
        mod = p % dy;
        if (mod < 0) {
          delta--;
          mod += dy;
        }

        x_from = x1 + delta;
        RenderHLine(ey1, x1, fy1, x_from, first);
        ey1 += incr;
        //***
        AddNewCell(x_from >> POLY_SUBPIXEL_SHIFT, ey1);
        //***
        if (ey1 != ey2) {
          p = POLY_SUBPIXEL_SCALE * dx;
          lift = p / dy;
          rem = p % dy;
          if (rem < 0) {
            lift--;
            rem += dy;
          }
          mod -= dy;
          while (ey1 != ey2) {
            delta = lift;
            mod += rem;
            if (mod >= 0) {
              mod -= dy;
              delta++;
            }

            x_to = x_from + delta;
            //***
            RenderHLine(ey1, x_from, POLY_SUBPIXEL_SCALE - first, x_to, first);
            //***
            x_from = x_to;
            ey1 += incr;
            //***
            AddNewCell(x_from >> POLY_SUBPIXEL_SHIFT, ey1);
            //***
          }
        }
        RenderHLine(ey1, x_from, POLY_SUBPIXEL_SCALE - first, x2, fy2);
      }



      public int MinX { get; private set; }
      public int MinY { get; private set; }
      public int MaxX { get; private set; }
      public int MaxY { get; private set; }

      public void SortCells() {
        if (Sorted) return; //Perform sort only the first time.
        WriteCurrentCell();
        //----------------------------------
        //reset current cell 
        cCell_x = 0x7FFFFFFF;
        cCell_y = 0x7FFFFFFF;
        cCell_cover = 0;
        cCell_area = 0;
        //----------------------------------

        if (TotalCells == 0) return;
        // Allocate the array of cell pointers 
        m_sorted_cells.Allocate(TotalCells);
        // Allocate and zero the Y array
        m_sorted_y.Allocate((int)(MaxY - MinY + 1));
        m_sorted_y.Zero();
        CellAA[] cells = m_cells.Array;
        SortedY[] sortedYData = m_sorted_y.Array;
        CellAA[] sortedCellsData = m_sorted_cells.Array;
        // Create the Y-histogram (count the numbers of cells for each Y)
        for (int i = 0; i < TotalCells; ++i) {
          int index = cells[i].y - MinY;
          sortedYData[index].start++;
        }

        // Convert the Y-histogram into the array of starting indexes
        int start = 0;
        int sortedYSize = m_sorted_y.Count;
        for (int i = 0; i < sortedYSize; i++) {
          int v = sortedYData[i].start;
          sortedYData[i].start = start;
          start += v;
        }

        // Fill the cell pointer array sorted by Y
        for (int i = 0; i < TotalCells; ++i) {
          int sortedIndex = cells[i].y - MinY;
          int curr_y_start = sortedYData[sortedIndex].start;
          int curr_y_num = sortedYData[sortedIndex].num;
          sortedCellsData[curr_y_start + curr_y_num] = cells[i];
          sortedYData[sortedIndex].num++;
        }

        // Finally arrange the X-arrays
        for (int i = 0; i < sortedYSize; i++) {
          var yData = sortedYData[i];
          if (yData.num != 0) {
            QuickSort.Sort(sortedCellsData,
                yData.start,
                yData.start + yData.num - 1);
          }
        }
        Sorted = true;
      }

      public int TotalCells { get; private set; }


      public void GetCells(int y, out CellAA[] cellData, out int offset, out int num) {
        cellData = m_sorted_cells.Array;
        SortedY d = m_sorted_y[y - MinY];
        offset = d.start;
        num = d.num;
      }


      public bool Sorted { get; private set; }

      void AddNewCell(int x, int y) {
        WriteCurrentCell();
        cCell_x = x;
        cCell_y = y;
        //reset area and coverage after add new cell
        cCell_cover = 0;
        cCell_area = 0;
      }

      void WriteCurrentCell() {
        if ((cCell_area | cCell_cover) != 0) {
          //check cell limit
          if (TotalCells >= BLOCK_LIMIT) {
            return;
          }
          //------------------------------------------
          //alloc if required
          if ((TotalCells + 1) >= m_cells.AllocatedSize) {
            m_cells = new DrawingList<CellAA>(m_cells, BLOCK_SIZE);
          }
          m_cells.SetData(TotalCells, new CellAA(
          cCell_x, cCell_y,
          cCell_cover, cCell_area));
          TotalCells++;
        }
      }

      void RenderHLine(int ey, int x1, int y1, int x2, int y2) {

        //trivial case. Happens often
        if (y1 == y2) {
          //***
          AddNewCell(x2 >> PolySubPix.SHIFT, ey);
          //***
          return;
        }
        int ex1 = x1 >> PolySubPix.SHIFT;
        int ex2 = x2 >> PolySubPix.SHIFT;

        int fx1 = x1 & (int)PolySubPix.MASK;
        int fx2 = x2 & (int)PolySubPix.MASK;
        int delta;
        //everything is located in a single cell.  That is easy!
        if (ex1 == ex2) {
          delta = y2 - y1;
          cCell_cover += delta;
          cCell_area += (fx1 + fx2) * delta;
          return;
        }
        //----------------------------
        int p, first, dx;
        int incr, lift, mod, rem;
        //----------------------------


        //ok, we'll have to render a run of adjacent cells on the same hline...
        p = ((int)PolySubPix.SCALE - fx1) * (y2 - y1);
        first = (int)PolySubPix.SCALE;
        incr = 1;
        dx = x2 - x1;
        if (dx < 0) {
          p = fx1 * (y2 - y1);
          first = 0;
          incr = -1;
          dx = -dx;
        }

        delta = p / dx;
        mod = p % dx;
        if (mod < 0) {
          delta--;
          mod += dx;
        }

        cCell_cover += delta;
        cCell_area += (fx1 + first) * delta;
        ex1 += incr;
        //***
        AddNewCell(ex1, ey);
        //***
        y1 += delta;
        if (ex1 != ex2) {
          p = (int)PolySubPix.SCALE * (y2 - y1 + delta);
          lift = p / dx;
          rem = p % dx;
          if (rem < 0) {
            lift--;
            rem += dx;
          }

          mod -= dx;
          while (ex1 != ex2) {
            delta = lift;
            mod += rem;
            if (mod >= 0) {
              mod -= dx;
              delta++;
            }

            cCell_cover += delta;
            cCell_area += (int)PolySubPix.SCALE * delta;
            y1 += delta;
            ex1 += incr;
            //***
            AddNewCell(ex1, ey);
            //***
          }
        }
        delta = y2 - y1;
        cCell_cover += delta;
        cCell_area += (fx2 + (int)PolySubPix.SCALE - first) * delta;
      }

      //------------
      static class QuickSort {
        public static void Sort(CellAA[] dataToSort) {
          Sort(dataToSort, 0, dataToSort.Length - 1);
        }

        public static void Sort(CellAA[] dataToSort, int beg, int end) {
          if (end == beg) {
            return;
          } else {
            int pivot = GetPivotPoint(dataToSort, beg, end);
            if (pivot > beg) {
              Sort(dataToSort, beg, pivot - 1);
            }

            if (pivot < end) {
              Sort(dataToSort, pivot + 1, end);
            }
          }
        }

        static int GetPivotPoint(CellAA[] dataToSort, int begPoint, int endPoint) {
          int pivot = begPoint;
          int m = begPoint + 1;
          int n = endPoint;
          var x_at_PivotPoint = dataToSort[pivot].x;
          while ((m < endPoint)
              && x_at_PivotPoint >= dataToSort[m].x) {
            m++;
          }

          while ((n > begPoint) && (x_at_PivotPoint <= dataToSort[n].x)) {
            n--;
          }

          while (m < n) {
            //swap data between m and n
            CellAA temp = dataToSort[m];
            dataToSort[m] = dataToSort[n];
            dataToSort[n] = temp;
            while ((m < endPoint) && (x_at_PivotPoint >= dataToSort[m].x)) {
              m++;
            }

            while ((n > begPoint) && (x_at_PivotPoint <= dataToSort[n].x)) {
              n--;
            }
          }

          if (pivot != n) {
            CellAA temp2 = dataToSort[n];
            dataToSort[n] = dataToSort[pivot];
            dataToSort[pivot] = temp2;
          }
          return n;
        }
      }
    }

    public struct RectInt {
      public int Left, Bottom, Right, Top;
      public RectInt(int left, int bottom, int right, int top) {
        Left = left;
        Bottom = bottom;
        Right = right;
        Top = top;
      }


      // This function assumes the rect is normalized
      public int Width => Right - Left;

      // This function assumes the rect is normalized
      public int Height => Top - Bottom;

      public void Normalize() {
        int t;
        if (Left > Right) { t = Left; Left = Right; Right = t; }
        if (Bottom > Top) { t = Bottom; Bottom = Top; Top = t; }
      }

      public void ExpandToInclude(RectInt rectToInclude) {
        if (Right < rectToInclude.Right) Right = rectToInclude.Right;
        if (Top < rectToInclude.Top) Top = rectToInclude.Top;
        if (Left > rectToInclude.Left) Left = rectToInclude.Left;
        if (Bottom > rectToInclude.Bottom) Bottom = rectToInclude.Bottom;
      }

      public bool Clip(RectInt r) {
        if (Right > r.Right) Right = r.Right;
        if (Top > r.Top) Top = r.Top;
        if (Left < r.Left) Left = r.Left;
        if (Bottom < r.Bottom) Bottom = r.Bottom;
        return Left <= Right && Bottom <= Top;
      }

      public bool Valid => Left <= Right && Bottom <= Top;

      public bool Contains(int x, int y) => x >= Left && x <= Right && y >= Bottom && y <= Top;

      public bool IntersectRectangles(RectInt rectToCopy, RectInt rectToIntersectWith) {
        Left = rectToCopy.Left;
        Bottom = rectToCopy.Bottom;
        Right = rectToCopy.Right;
        Top = rectToCopy.Top;
        if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
        if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
        if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
        if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;
        if (Left < Right && Bottom < Top) {
          return true;
        }

        return false;
      }

      public bool IntersectWithRectangle(RectInt rectToIntersectWith) {
        if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
        if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
        if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
        if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;
        if (Left < Right && Bottom < Top) {
          return true;
        }

        return false;
      }

      public static bool DoIntersect(RectInt rect1, RectInt rect2) {
        int x1 = rect1.Left;
        int y1 = rect1.Bottom;
        int x2 = rect1.Right;
        int y2 = rect1.Top;
        if (x1 < rect2.Left) x1 = rect2.Left;
        if (y1 < rect2.Bottom) y1 = rect2.Bottom;
        if (x2 > rect2.Right) x2 = rect2.Right;
        if (y2 > rect2.Top) y2 = rect2.Top;
        if (x1 < x2 && y1 < y2) {
          return true;
        }

        return false;
      }


      //---------------------------------------------------------unite_rectangles
      public void UniteRectangles(RectInt r1, RectInt r2) {
        Left = r1.Left;
        Bottom = r1.Bottom;
        Right = r1.Right;
        Right = r1.Top;
        if (Right < r2.Right) Right = r2.Right;
        if (Top < r2.Top) Top = r2.Top;
        if (Left > r2.Left) Left = r2.Left;
        if (Bottom > r2.Bottom) Bottom = r2.Bottom;
      }

      public void Inflate(int inflateSize) {
        Left = Left - inflateSize;
        Bottom = Bottom - inflateSize;
        Right = Right + inflateSize;
        Top = Top + inflateSize;
      }

      public void Offset(int x, int y) {
        Left = Left + x;
        Bottom = Bottom + y;
        Right = Right + x;
        Top = Top + y;
      }

      public override int GetHashCode() => (Left, Right, Bottom, Top).GetHashCode();

      public static bool ClipRects(RectInt pBoundingRect, ref RectInt pSourceRect, ref RectInt pDestRect) {
        // clip off the top so we don't write into random memory
        if (pDestRect.Top < pBoundingRect.Top) {
          // This type of clipping only works when we aren't scaling an image...
          // If we are scaling an image, the source and dest sizes won't match
          if (pSourceRect.Height != pDestRect.Height)
            throw new Exception("source and dest rects must have the same height");

          pSourceRect.Top += pBoundingRect.Top - pDestRect.Top;
          pDestRect.Top = pBoundingRect.Top;
          if (pDestRect.Top >= pDestRect.Bottom) {
            return false;
          }
        }
        // clip off the bottom
        if (pDestRect.Bottom > pBoundingRect.Bottom) {
          // This type of clipping only works when we arenst scaling an image...
          // If we are scaling an image, the source and desst sizes won't match
          if (pSourceRect.Height != pDestRect.Height) {
            throw new Exception("source and dest rects must have the same height");
          }

          pSourceRect.Bottom -= pDestRect.Bottom - pBoundingRect.Bottom;
          pDestRect.Bottom = pBoundingRect.Bottom;
          if (pDestRect.Bottom <= pDestRect.Top) {
            return false;
          }
        }

        // clip off the left
        if (pDestRect.Left < pBoundingRect.Left) {
          // This type of clipping only works when we aren't scaling an image...
          // If we are scaling an image, the source and dest sizes won't match
          if (pSourceRect.Width != pDestRect.Width) {
            throw new Exception("source and dest rects must have the same width");
          }

          pSourceRect.Left += pBoundingRect.Left - pDestRect.Left;
          pDestRect.Left = pBoundingRect.Left;
          if (pDestRect.Left >= pDestRect.Right) {
            return false;
          }
        }
        // clip off the right
        if (pDestRect.Right > pBoundingRect.Right) {
          // This type of clipping only works when we aren't scaling an image...
          // If we are scaling an image, the source and dest sizes won't match
          if (pSourceRect.Width != pDestRect.Width) {
            throw new Exception("source and dest rects must have the same width");
          }

          pSourceRect.Right -= pDestRect.Right - pBoundingRect.Right;
          pDestRect.Right = pBoundingRect.Right;
          if (pDestRect.Right <= pDestRect.Left) {
            return false;
          }
        }

        return true;
      }


      //***************************************************************************************************************************************************
      public static bool ClipRect(RectInt pBoundingRect, ref RectInt pDestRect) {
        // clip off the top so we don't write into random memory
        if (pDestRect.Top < pBoundingRect.Top) {
          pDestRect.Top = pBoundingRect.Top;
          if (pDestRect.Top >= pDestRect.Bottom) {
            return false;
          }
        }
        // clip off the bottom
        if (pDestRect.Bottom > pBoundingRect.Bottom) {
          pDestRect.Bottom = pBoundingRect.Bottom;
          if (pDestRect.Bottom <= pDestRect.Top) {
            return false;
          }
        }

        // clip off the left
        if (pDestRect.Left < pBoundingRect.Left) {
          pDestRect.Left = pBoundingRect.Left;
          if (pDestRect.Left >= pDestRect.Right) {
            return false;
          }
        }

        // clip off the right
        if (pDestRect.Right > pBoundingRect.Right) {
          pDestRect.Right = pBoundingRect.Right;
          if (pDestRect.Right <= pDestRect.Left) {
            return false;
          }
        }

        return true;
      }
    }

    public static class ClipLiangBarsky {
      //------------------------------------------------------------------------
      static class ClippingFlags {
        public const int cX1 = 4; // x1 clipped
        public const int cX2 = 1; //x2 clipped
        public const int cY1 = 8; //y1 clipped
        public const int cY2 = 2; //y2 clipped
        public const int cX1X2 = cX1 | cX2;
        public const int cY1Y2 = cY1 | cY2;
      }

      //----------------------------------------------------------clipping_flags
      // Determine the clipping code of the vertex according to the 
      // Cyrus-Beck line clipping algorithm
      //
      //        |        |
      //  0110  |  0010  | 0011
      //        |        |
      // -------+--------+-------- clip_box.y2
      //        |        |
      //  0100  |  0000  | 0001
      //        |        |
      // -------+--------+-------- clip_box.y1
      //        |        |
      //  1100  |  1000  | 1001
      //        |        |
      //  clip_box.x1  clip_box.x2
      //
      // 
      //template<class T>
      public static int Flags(int x, int y, RectInt clip_box) =>
        ((x > clip_box.Right) ? 1 : 0) |
        ((y > clip_box.Top) ? 1 << 1 : 0) |
        ((x < clip_box.Left) ? 1 << 2 : 0) |
        ((y < clip_box.Bottom) ? 1 << 3 : 0);

      public static int GetFlagsX(int x, RectInt clip_box) => (x > clip_box.Right ? 1 : 0) | ((x < clip_box.Left ? 1 : 0) << 2);

      public static int GetFlagsY(int y, RectInt clip_box) => (((y > clip_box.Top ? 1 : 0) << 1) | ((y < clip_box.Bottom ? 1 : 0) << 3));

      public static int DoClipLiangBarsky(int x1, int y1, int x2, int y2,
                                        RectInt clip_box,
                                        int[] x, int[] y) {
        int xIndex = 0;
        int yIndex = 0;
        double nearzero = 1e-30;
        double deltax = x2 - x1;
        double deltay = y2 - y1;
        double xin;
        double xout;
        double yin;
        double yout;
        double tinx;
        double tiny;
        double toutx;
        double touty;
        double tin1;
        double tin2;
        double tout1;
        int np = 0;
        if (deltax == 0.0) {
          // bump off of the vertical
          deltax = (x1 > clip_box.Left) ? -nearzero : nearzero;
        }

        if (deltay == 0.0) {
          // bump off of the horizontal 
          deltay = (y1 > clip_box.Bottom) ? -nearzero : nearzero;
        }

        if (deltax > 0.0) {
          // points to right
          xin = clip_box.Left;
          xout = clip_box.Right;
        } else {
          xin = clip_box.Right;
          xout = clip_box.Left;
        }

        if (deltay > 0.0) {
          // points up
          yin = clip_box.Bottom;
          yout = clip_box.Top;
        } else {
          yin = clip_box.Top;
          yout = clip_box.Bottom;
        }

        tinx = (xin - x1) / deltax;
        tiny = (yin - y1) / deltay;
        if (tinx < tiny) {
          // hits x first
          tin1 = tinx;
          tin2 = tiny;
        } else {
          // hits y first
          tin1 = tiny;
          tin2 = tinx;
        }

        if (tin1 <= 1.0) {
          if (0.0 < tin1) {
            x[xIndex++] = (int)xin;
            y[yIndex++] = (int)yin;
            ++np;
          }

          if (tin2 <= 1.0) {
            toutx = (xout - x1) / deltax;
            touty = (yout - y1) / deltay;
            tout1 = (toutx < touty) ? toutx : touty;
            if (tin2 > 0.0 || tout1 > 0.0) {
              if (tin2 <= tout1) {
                if (tin2 > 0.0) {
                  if (tinx > tiny) {
                    x[xIndex++] = (int)xin;
                    y[yIndex++] = (int)(y1 + tinx * deltay);
                  } else {
                    x[xIndex++] = (int)(x1 + tiny * deltax);
                    y[yIndex++] = (int)yin;
                  }
                  ++np;
                }

                if (tout1 < 1.0) {
                  if (toutx < touty) {
                    x[xIndex++] = (int)xout;
                    y[yIndex++] = (int)(y1 + toutx * deltay);
                  } else {
                    x[xIndex++] = (int)(x1 + touty * deltax);
                    y[yIndex++] = (int)yout;
                  }
                } else {
                  x[xIndex++] = x2;
                  y[yIndex++] = y2;
                }
                ++np;
              } else {
                if (tinx > tiny) {
                  x[xIndex++] = (int)xin;
                  y[yIndex++] = (int)yout;
                } else {
                  x[xIndex++] = (int)xout;
                  y[yIndex++] = (int)yin;
                }
                ++np;
              }
            }
          }
        }
        return np;
      }

      public static bool ClipMovePoint(int x1, int y1, int x2, int y2,
                           RectInt clip_box,
                           ref int x, ref int y, int flags) {
        int bound;
        if ((flags & ClippingFlags.cX1X2) != 0) {
          if (x1 == x2) {
            return false;
          }
          bound = ((flags & ClippingFlags.cX1) != 0) ? clip_box.Left : clip_box.Right;
          y = (int)((double)(bound - x1) * (y2 - y1) / (x2 - x1) + y1);
          x = bound;
        }

        flags = GetFlagsY(y, clip_box);
        if ((flags & ClippingFlags.cY1Y2) != 0) {
          if (y1 == y2) {
            return false;
          }
          bound = ((flags & ClippingFlags.cY1) != 0) ? clip_box.Bottom : clip_box.Top;
          x = (int)((double)(bound - y1) * (x2 - x1) / (y2 - y1) + x1);
          y = bound;
        }
        return true;
      }

      //-------------------------------------------------------clip_line_segment
      // Returns: ret >= 4        - Fully clipped
      //          (ret & 1) != 0  - First point has been moved
      //          (ret & 2) != 0  - Second point has been moved
      //
      //template<class T>
      public static int ClipLineSegment(ref int x1, ref int y1, ref int x2, ref int y2,
                                 RectInt clip_box) {
        int f1 = Flags(x1, y1, clip_box);
        int f2 = Flags(x2, y2, clip_box);
        int ret = 0;
        if ((f2 | f1) == 0) {
          // Fully visible
          return 0;
        }

        if ((f1 & ClippingFlags.cX1X2) != 0 &&
           (f1 & ClippingFlags.cX1X2) == (f2 & ClippingFlags.cX1X2)) {
          // Fully clipped
          return 4;
        }

        if ((f1 & ClippingFlags.cY1Y2) != 0 &&
           (f1 & ClippingFlags.cY1Y2) == (f2 & ClippingFlags.cY1Y2)) {
          // Fully clipped
          return 4;
        }

        int tx1 = x1;
        int ty1 = y1;
        int tx2 = x2;
        int ty2 = y2;
        if (f1 != 0) {
          if (!ClipMovePoint(tx1, ty1, tx2, ty2, clip_box, ref x1, ref y1, f1)) {
            return 4;
          }
          if (x1 == x2 && y1 == y2) {
            return 4;
          }
          ret |= 1;
        }
        if (f2 != 0) {
          if (!ClipMovePoint(tx1, ty1, tx2, ty2, clip_box, ref x2, ref y2, f2)) {
            return 4;
          }
          if (x1 == x2 && y1 == y2) {
            return 4;
          }
          ret |= 2;
        }
        return ret;
      }
    }

    class VectorClipper {
      RectInt clipBox;
      int m_x1;
      int m_y1;
      int m_f1;
      bool m_clipping;
      CellAARasterizer ras;
      public VectorClipper(CellAARasterizer ras) {
        this.ras = ras;
        clipBox = new RectInt(0, 0, 0, 0);
        m_x1 = m_y1 = m_f1 = 0;
        m_clipping = false;
      }
      public RectInt GetVectorClipBox() => clipBox;

      public void SetClipBox(int x1, int y1, int x2, int y2) {
        clipBox = new RectInt(x1, y1, x2, y2);
        clipBox.Normalize();
        m_clipping = true;
      }

      /// <summary>
      /// clip box width is extened 3 times for lcd-effect subpixel rendering
      /// </summary>
      bool _clipBoxWidthX3ForSubPixelLcdEffect = false; //default

      /// <summary>
      /// when we render in subpixel rendering, we extend a row length 3 times (expand RGB)
      /// </summary>
      /// <param name="value"></param>
      public void SetClipBoxWidthX3ForSubPixelLcdEffect(bool value) {
        //-----------------------------------------------------------------------------
        //if we don't want to expand our img buffer 3 times (larger than normal)
        //we should use this method to extend only a cliper box's width x3                 
        //-----------------------------------------------------------------------------

        //special method for our need
        if (value != _clipBoxWidthX3ForSubPixelLcdEffect) {
          //changed
          if (value) {
            clipBox = new RectInt(clipBox.Left, clipBox.Bottom, clipBox.Left + (clipBox.Width * 3), clipBox.Height);
          } else {
            //set back
            clipBox = new RectInt(clipBox.Left, clipBox.Bottom, clipBox.Left + (clipBox.Width / 3), clipBox.Height);
          }
          _clipBoxWidthX3ForSubPixelLcdEffect = value;
        }
      }

      public void ResetClipping() => m_clipping = false;
      public void MoveTo(int x1, int y1) {
        m_x1 = x1;
        m_y1 = y1;
        if (m_clipping)
          m_f1 = ClipLiangBarsky.Flags(x1, y1, clipBox);
      }

      //------------------------------------------------------------------------
      void LineClipY(int x1, int y1,
                     int x2, int y2,
                     int f1, int f2) {
        f1 &= 10;
        f2 &= 10;
        if ((f1 | f2) == 0) {
          // Fully visible
          ras.DrawLine(x1, y1, x2, y2);
        } else {
          if (f1 == f2) {
            // Invisible by Y
            return;
          }

          int tx1 = x1;
          int ty1 = y1;
          int tx2 = x2;
          int ty2 = y2;
          if ((f1 & 8) != 0) // y1 < clip.y1
          {
            tx1 = x1 + MulDiv(clipBox.Bottom - y1, x2 - x1, y2 - y1);
            ty1 = clipBox.Bottom;
          }

          if ((f1 & 2) != 0) // y1 > clip.y2
          {
            tx1 = x1 + MulDiv(clipBox.Top - y1, x2 - x1, y2 - y1);
            ty1 = clipBox.Top;
          }

          if ((f2 & 8) != 0) // y2 < clip.y1
          {
            tx2 = x1 + MulDiv(clipBox.Bottom - y1, x2 - x1, y2 - y1);
            ty2 = clipBox.Bottom;
          }

          if ((f2 & 2) != 0) // y2 > clip.y2
          {
            tx2 = x1 + MulDiv(clipBox.Top - y1, x2 - x1, y2 - y1);
            ty2 = clipBox.Top;
          }

          ras.DrawLine(tx1, ty1, tx2, ty2);
        }
      }

      //--------------------------------------------------------------------
      public void LineTo(int x2, int y2) {
        if (m_clipping) {
          int f2 = ClipLiangBarsky.Flags(x2, y2, clipBox);
          if ((m_f1 & 10) == (f2 & 10) && (m_f1 & 10) != 0) {
            // Invisible by Y
            m_x1 = x2;
            m_y1 = y2;
            m_f1 = f2;
            return;
          }

          int x1 = m_x1;
          int y1 = m_y1;
          int f1 = m_f1;
          int y3, y4;
          int f3, f4;
          switch (((f1 & 5) << 1) | (f2 & 5)) {
            case 0: // Visible by X
              LineClipY(x1, y1, x2, y2, f1, f2);
              break;
            case 1: // x2 > clip.x2
              y3 = y1 + MulDiv(clipBox.Right - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              LineClipY(x1, y1, clipBox.Right, y3, f1, f3);
              LineClipY(clipBox.Right, y3, clipBox.Right, y2, f3, f2);
              break;
            case 2: // x1 > clip.x2
              y3 = y1 + MulDiv(clipBox.Right - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              LineClipY(clipBox.Right, y1, clipBox.Right, y3, f1, f3);
              LineClipY(clipBox.Right, y3, x2, y2, f3, f2);
              break;
            case 3: // x1 > clip.x2 && x2 > clip.x2
              LineClipY(clipBox.Right, y1, clipBox.Right, y2, f1, f2);
              break;
            case 4: // x2 < clip.x1
              y3 = y1 + MulDiv(clipBox.Left - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              LineClipY(x1, y1, clipBox.Left, y3, f1, f3);
              LineClipY(clipBox.Left, y3, clipBox.Left, y2, f3, f2);
              break;
            case 6: // x1 > clip.x2 && x2 < clip.x1
              y3 = y1 + MulDiv(clipBox.Right - x1, y2 - y1, x2 - x1);
              y4 = y1 + MulDiv(clipBox.Left - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              f4 = ClipLiangBarsky.GetFlagsY(y4, clipBox);
              LineClipY(clipBox.Right, y1, clipBox.Right, y3, f1, f3);
              LineClipY(clipBox.Right, y3, clipBox.Left, y4, f3, f4);
              LineClipY(clipBox.Left, y4, clipBox.Left, y2, f4, f2);
              break;
            case 8: // x1 < clip.x1
              y3 = y1 + MulDiv(clipBox.Left - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              LineClipY(clipBox.Left, y1, clipBox.Left, y3, f1, f3);
              LineClipY(clipBox.Left, y3, x2, y2, f3, f2);
              break;
            case 9:  // x1 < clip.x1 && x2 > clip.x2
              y3 = y1 + MulDiv(clipBox.Left - x1, y2 - y1, x2 - x1);
              y4 = y1 + MulDiv(clipBox.Right - x1, y2 - y1, x2 - x1);
              f3 = ClipLiangBarsky.GetFlagsY(y3, clipBox);
              f4 = ClipLiangBarsky.GetFlagsY(y4, clipBox);
              LineClipY(clipBox.Left, y1, clipBox.Left, y3, f1, f3);
              LineClipY(clipBox.Left, y3, clipBox.Right, y4, f3, f4);
              LineClipY(clipBox.Right, y4, clipBox.Right, y2, f4, f2);
              break;
            case 12: // x1 < clip.x1 && x2 < clip.x1
              LineClipY(clipBox.Left, y1, clipBox.Left, y2, f1, f2);
              break;
          }
          m_f1 = f2;
        } else {
          ras.DrawLine(m_x1, m_y1,
                   x2, y2);
        }
        m_x1 = x2;
        m_y1 = y2;
      }


      static int IRound(float v) => unchecked((int)((v < 0.0) ? v - 0.5 : v + 0.5));
      static int MulDiv(int a, int b, int c) => IRound((float)a * b / c);
    }
  }
}
