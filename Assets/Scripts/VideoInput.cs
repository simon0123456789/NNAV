//#define GRAYSCALE

//https://towardsdatascience.com/extracting-regions-of-interest-from-images-dacfd05a41ba

//https://opencv24-python-tutorials.readthedocs.io/en/stable/py_tutorials/py_imgproc/py_filtering/py_filtering.html


// Mycket bra om Houghs
// https://docs.opencv.org/3.4/d9/db0/tutorial_hough_lines.html

/* Bra Värden
 * Theata = 180
 * Line Detection Thres Hold = 56
 * Min Line Lenght = 170
 * Max Line Gap = 2
 */


using OpenCvSharp;
using System.Collections.Generic;
using UnityEngine;


public class VideoInput : MonoBehaviour
{

    // Color Format R32_UINT -> Också svart vit kanske till och med bättre för edge
    // Color Format R16_UNIFORM -> Svartvit output
    // Color Format R4G4B4A4_UNORM_PACK16 -> Ger bra bild men saker som inte ska vara med följer med ändå
    // Color Format vi hade från början: R8G8B8A8_UNORM

    public float leftTilt = 0f;
    public float rightTilt = 0f;

    public bool useGrayScale = true;
    public bool blurr = true;

    // Line Detection Parameters
    [Range(1, 360)]
    public double theta = 180;
    [Range(1f, 100f)]
    public int LineDetectionThresHold = 56;
    [Range(1, 200)]
    public int minLineLength = 170;
    [Range(1, 200)]
    public int maxLineGap = 2;

    // Edge Detection Thresholds :)
    [Range(0.5f, 400)]
    public double edgeThreshold1 = 170; //was 50 on race track;
    [Range(1, 400)]
    public double edgeThreshold2 = 200;

    //Vector2 roiSize = new Vector2(260f, 325f);
    [Range(1f, 600f)]
    public float roiWidth = 260f;
    [Range(1f, 600f)]
    public float roiHeight = 325f;

    public float roiLeftXPos = 0f;
    public float roiRightXPos = 380f;

    float originalRoiWidth = 260f;
    float originalRoiHeight = 325f;

    // Lines that we are going to use as input
    public OpenCvSharp.LineSegmentPoint lineRightSide;
    public OpenCvSharp.LineSegmentPoint lineLeftSide;

    // All detected lines, 
    // currently we only use the longest line :)
    OpenCvSharp.LineSegmentPoint[] linesRightSide;
    OpenCvSharp.LineSegmentPoint[] linesLeftSide;

    public bool drawVideoInput = false;
    public bool drawLines = false;

    // Gui
    List<Texture2D> guiDrawBuf;

    // Gui Settings
    [Range(1, 5)]
    public int guiTexturesScale = 1;
    public int guiXoffset = 410;
    public int guiYoffset = 70;
    public bool drawLeftLine = true;
    public bool drawRightLine = true;




    void gdd()
    {
        Camera Cam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;
        Cam.Render();

        //Texture2D 
        Texture2D camImage = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height, TextureFormat.ARGB32, false, true);

        camImage.ReadPixels(new UnityEngine.Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
        camImage.Apply();
        RenderTexture.active = currentRT;

    }

    #region Backend functions
    //Texture2D camImage;
    /// <summary>
    /// Grabs the current frame from the camera component.
    /// </summary>
    /// <returns>A 2D Texture of what the camera sees</returns>
    Texture2D GrabCameraFrame()
    {
        Camera Cam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;
        Cam.Render();

        //Texture2D 
        Texture2D camImage = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height, TextureFormat.ARGB32, false, true);

        camImage.ReadPixels(new UnityEngine.Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
        camImage.Apply();
        RenderTexture.active = currentRT;

        return camImage;
    }

    Mat ToGrayScale(Mat cvtInput) // Not in use
    {
        Mat cvtOutput = new Mat();
        Cv2.CvtColor(cvtInput, cvtOutput, ColorConversionCodes.BGR2GRAY); //BGR2GRAY
        return cvtOutput;
    }

    Mat Blurr(Mat gaussianBlurInput) // Not in use
    {
        Mat gaussianBlurOutput = new Mat();
        Cv2.GaussianBlur(gaussianBlurInput, gaussianBlurOutput, new Size(5, 5), 0);
        return gaussianBlurOutput;
    }

    Mat ExtractEdges(Mat cannyInput)
    {
        Mat cannyOutput = new Mat();
        Cv2.Canny(cannyInput, cannyOutput, edgeThreshold1, edgeThreshold2);
        return cannyOutput;
    }

    Mat RegionOfInterest(Mat input, int x, int y, int width, int height)
    {
        OpenCvSharp.Rect roiRect = new OpenCvSharp.Rect(x, y, width, height);
        return new Mat(input, roiRect);
    }

    Mat Dilate(Mat input)
    {
        Mat output = new Mat();
        Cv2.Dilate(input, output, new Mat());
        return output;
    }

    OpenCvSharp.LineSegmentPoint[] ExtractLines(Mat houghInput) // Third call
    {
        return Cv2.HoughLinesP(
            image: houghInput,
            rho: 1,
            theta: Mathf.PI / theta,
            threshold: LineDetectionThresHold,
            minLineLength: minLineLength,
            maxLineGap: maxLineGap);
    }



    double linesFound = 0, linesNotFound = 0;
    OpenCvSharp.LineSegmentPoint ExtractLongestLine(Mat houghInput)
    {
        OpenCvSharp.LineSegmentPoint[] themLines = ExtractLines(houghInput);
        if (themLines.Length < 1)
        {
            linesNotFound++;
            return new OpenCvSharp.LineSegmentPoint();
        }


        OpenCvSharp.LineSegmentPoint longestLine = themLines[0];
        float longestDist = 0;
        for (int i = 0; i < themLines.Length; i++)
        {
            float dist = Vector2.Distance(new Vector2(themLines[i].P1.X, themLines[i].P1.Y), new Vector2(themLines[i].P2.X, themLines[i].P2.Y));
            if (dist > longestDist)
            {
                longestLine = themLines[i];
                longestDist = dist;
            }
        }
        linesFound++;
        return longestLine;
    }

    #endregion

    float NormalizedTilt(LineSegmentPoint point, Vector2 camRes)
    {
        float camCenterX = camRes.x / 2f;
        float camCenterY = camRes.y / 2f;

        float x1 = point.P1.X - camCenterX;
        float x2 = point.P2.X - camCenterX;

        float y1 = point.P1.Y - camCenterY;
        float y2 = point.P2.Y - camCenterY;

        float x1Normalized = x1 / camCenterX;
        float x2Normalized = x2 / camCenterX;

        float y1Normalized = y1 / camCenterY;
        float y2Normalized = y2 / camCenterY;

        float k = (y2Normalized - y1Normalized) / (x2Normalized - x1Normalized);

        return k;
    }

    public bool efficientMode = false;
    void Update()
    //public void DoLineDetection(ref float left, ref float right)
    {
        Texture2D currentFrame = GrabCameraFrame();
        
        Mat currentFrameMat = useGrayScale ? ToGrayScale(OpenCvSharp.Unity.TextureToMat(currentFrame)) : OpenCvSharp.Unity.TextureToMat(currentFrame);
        Mat edges = blurr ?  ExtractEdges(Blurr(currentFrameMat)) : ExtractEdges(currentFrameMat);

        int roiYPos = (roiHeight + originalRoiHeight > 640) ? (int)originalRoiHeight - (((int)roiHeight + (int)originalRoiHeight) - 640) : (int)originalRoiHeight;
        Mat edgesLeftSide   = RegionOfInterest(edges, x: 0, y: roiYPos, width: (int)roiWidth, height: (int)roiHeight );
        int roiXPos = (roiWidth+ originalRoiWidth > 640 ) ? (int)originalRoiWidth - (((int)roiWidth+(int) originalRoiWidth) -640) : (int)originalRoiWidth;
        Mat edgesRightSide  = RegionOfInterest(edges, x: roiXPos, y: roiYPos, width: (int)roiWidth, height: (int)roiHeight);
            //Debug.Log("roiYPos = " + roiYPos + "-------- roiXPos = " + roiXPos);
        // Dilate makes the lines thicc(er)
        Mat rightDilate = Dilate(edgesRightSide);
        Mat leftDilate = Dilate(edgesLeftSide);

        lineRightSide = ExtractLongestLine(rightDilate);
        lineLeftSide = ExtractLongestLine(leftDilate);
        
        rightTilt = NormalizedTilt(lineRightSide, new Vector2(roiWidth, roiHeight));
        leftTilt = NormalizedTilt(lineLeftSide, new Vector2(roiWidth, roiHeight));
        //left = NormalizedTilt(lineLeftSide, new Vector2(roiWidth, roiHeight), leftEye: true);
        //right = NormalizedTilt(lineRightSide, new Vector2(roiWidth, roiHeight), leftEye: false);

        if (drawVideoInput)
        {
           // Texture2D leftSideEye = OpenCvSharp.Unity.MatToTexture(edgesLeftSide);
            //Texture2D rightSideEye = OpenCvSharp.Unity.MatToTexture(edgesRightSide);

            Texture2D leftSideEye   = OpenCvSharp.Unity.MatToTexture(RegionOfInterest(currentFrameMat, x: 0, y: roiYPos, width: (int)roiWidth, height: (int)roiHeight));
            Texture2D rightSideEye  = OpenCvSharp.Unity.MatToTexture(RegionOfInterest(currentFrameMat, x: roiXPos, y: roiYPos, width: (int)roiWidth, height: (int)roiHeight));

            LineDrawer.Draw2DOnTexture(ref leftSideEye, lineLeftSide, Color.blue, thicc: 5f, smooth: 5f);
            LineDrawer.Draw2DOnTexture(ref rightSideEye, lineRightSide, Color.blue, thicc: 5f, smooth: 5f);

            guiDrawBuf.Clear();

            if (drawLeftLine)
                guiDrawBuf.Add(leftSideEye);

            if (drawRightLine)
                guiDrawBuf.Add(rightSideEye);
        }
        Destroy(currentFrame);
        
    }

    void OnGUI()
    {
        if (!drawVideoInput)
            return;

        for (int i = 0, totalWidth = 0; i < guiDrawBuf.Count; i++)
        {
            if (guiDrawBuf[i] == null)
                continue;
            int posX = totalWidth + guiXoffset;
            int posY = guiYoffset; //Screen.height - (GetComponent<Camera>().pixelWidth / guiTexturesScale) - 80;
            int width = guiDrawBuf[i].width / guiTexturesScale;
            int height = guiDrawBuf[i].height / guiTexturesScale;

            GUI.DrawTexture(new UnityEngine.Rect(posX, posY, width, height), guiDrawBuf[i]);
            totalWidth += width + 20;
            Destroy(guiDrawBuf[i]);
            
        }
        
        GUI.Label(new UnityEngine.Rect(x: guiXoffset-60, y: guiYoffset, height: 50, width: 60), $"<size=20><b>{System.Math.Round(linesFound/(linesFound+linesNotFound)*100)}%</b></size>");
    }

    void Start()
    {
        guiDrawBuf = new List<Texture2D>();
    }
}
