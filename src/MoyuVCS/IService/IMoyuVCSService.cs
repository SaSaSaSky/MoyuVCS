namespace MoyuVCS.Service
{
    public interface IMoyuVCSService
    {
        void Update(string path);

        void Commit(string path);

        void ShowLog(string path);

        void Revert(string path);
    }
}
