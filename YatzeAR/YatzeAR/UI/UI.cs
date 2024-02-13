using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YatzeAR.YatzyLogik;

namespace YatzeAR.UI
{
	public static class UI
	{
		public static void DrawUserInfo(TurnHandler turnHandler, Mat frame)
		{
			try
			{
			
				turnHandler.Users.ForEach(x =>
				{
					
						//Rectangle rec = CvInvoke.BoundingRectangle(x.Contour);
						Point top = new Point(x.Contour.X + x.Contour.Width, x.Contour.Y);
						Point second = new Point(x.Contour.X + x.Contour.Width, x.Contour.Y + 40);
						Point third = new Point(x.Contour.X + x.Contour.Width, x.Contour.Y + 80);
						//CvInvoke.PutText(frame, "Current rule: " + turnHandler.CurrentRule.Rule, topOfSccreen, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), 1);
						//CvInvoke.PutText(frame, "Current user: " + turnHandler.currentUser.Name, beneath, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), 1);
						CvInvoke.PutText(frame, "Roll for " + turnHandler.CurrentRule.Rule, top, FontFace.HersheyPlain, 1.0, new MCvScalar(255, 0, 255), 1);
						//CvInvoke.PutText(frame, "Next user: "+turnHandler.currentUser.Name, top, FontFace.HersheyPlain, 1.0, new MCvScalar(255, 0, 255), 1);
						CvInvoke.PutText(frame, turnHandler.currentUser.Rules.Sum(i => i.Points).ToString(), top, FontFace.HersheyPlain, 1.0, new MCvScalar(255, 0, 255), 1);
					

				});
			}
			catch (Exception)
			{

				
			}
		
		}


	}
}
