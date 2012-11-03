using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Spider.Solitaire.View
{
    public class StoryboardPlayer
    {
        private Storyboard storyboard;
        private TaskCompletionSource<object> tcs;

        public StoryboardPlayer(Storyboard storyboard)
        {
            this.storyboard = storyboard;
        }

        public Task PlayStoryboardAsync()
        {
            tcs = new TaskCompletionSource<object>();
            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
            return tcs.Task;
        }

        void storyboard_Completed(object sender, EventArgs e)
        {
            storyboard.Completed -= storyboard_Completed;
            if (tcs != null)
            {
                tcs.SetResult(null);
                tcs = null;
            }
        }
    }
}
