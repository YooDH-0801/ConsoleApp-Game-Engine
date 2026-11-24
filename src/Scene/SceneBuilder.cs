namespace ConsoleApp_Game_Engine
{
	public static class SceneBuilder
	{
		/// <summary>
		/// 이 메소드에 씬을 등록하고 첫 씬을 불러오세요.
		/// </summary>
		public static void Build()
		{
			SceneManager.Register(new TitleScene());
			SceneManager.Register(new PlayScene());

			SceneManager.LoadScene("Title");
		}
	}
}