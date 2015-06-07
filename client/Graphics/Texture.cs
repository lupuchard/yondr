using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

public class Texture {
	
	public Texture(Res.Res res) {
		
		ID = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2D, ID);
		
		//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		
		GL.TexParameter(
			TextureTarget.Texture2D,
			TextureParameterName.TextureMinFilter,
			//(int)TextureMinFilter.LinearMipmapLinear
			(int)TextureMinFilter.Linear
		);
		GL.TexParameter(
			TextureTarget.Texture2D,
			TextureParameterName.TextureMagFilter,
			(int)TextureMagFilter.Linear
		);
		/*GL.TexParameter(
			TextureTarget.Texture2D,
			TextureParameterName.TextureWrapS,
			(int)TextureWrapMode.ClampToEdge
		);
		GL.TexParameter(
			TextureTarget.Texture2D,
			TextureParameterName.TextureWrapT,
			(int)TextureWrapMode.ClampToEdge
		);*/
		
		Bitmap bmp = new Bitmap(new MemoryStream(res.Data));
		BitmapData bmpData = bmp.LockBits(
			new Rectangle(0, 0, bmp.Width, bmp.Height),
			ImageLockMode.ReadOnly,
			System.Drawing.Imaging.PixelFormat.Format32bppArgb
		);

		if ((bmpData.Width  & (bmpData.Width  - 1)) != 0 ||
		    (bmpData.Height & (bmpData.Height - 1)) != 0) {
			Log.Warn("Texture {0} is not a power of 2.", res.Name);
		}
		
		GL.TexImage2D(
			TextureTarget.Texture2D, 0,
			PixelInternalFormat.Rgba,
			bmpData.Width, bmpData.Height, 0,
			OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
			PixelType.UnsignedByte,
			bmpData.Scan0
		);

		Util.CheckGL("texture");
	}

	~Texture() {
		GL.DeleteTexture(ID);
	}
	
	public int ID { get; }
	
}
