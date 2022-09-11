#include "iostream"
#include "windows.h"
using namespace std;
#define PIPE_NAME L"\\\\.\\Pipe\\test" 
void main()
{
	char buffer[1024];
	DWORD ReadNum;

	HANDLE hPipe = CreateNamedPipe(PIPE_NAME, PIPE_ACCESS_DUPLEX, PIPE_TYPE_BYTE | PIPE_READMODE_BYTE, 1, 0, 0, 1000, NULL);
	if (hPipe == INVALID_HANDLE_VALUE)
	{
		cout << "���������ܵ�ʧ�ܣ�" << endl;
		CloseHandle(hPipe);
		return;
	}

	if (ConnectNamedPipe(hPipe, NULL) == FALSE)
	{
		cout << "��ͻ�������ʧ�ܣ�" << endl;
		CloseHandle(hPipe);
		return;
	}
	cout << "��ͻ������ӳɹ���" << endl;

	while (1)
	{
		if (ReadFile(hPipe, buffer, 1024, &ReadNum, NULL) == FALSE)
		{
			cout << "��ȡ����ʧ�ܣ�" << endl;
			break;
		}

		buffer[ReadNum] = 0;
		cout << "��ȡ����:" << buffer << endl;
	}

	cout << "�رչܵ���" << endl;
	CloseHandle(hPipe);
	system("pause");
}
