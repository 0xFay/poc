#include "iostream"
#include "windows.h"
#include "stdio.h"
using namespace std;
#define PIPE_NAME L"\\\\.\\Pipe\\test"
void  main()
{
	char buffer[1024];
	DWORD WriteNum;
 
	if (WaitNamedPipe(PIPE_NAME, NMPWAIT_WAIT_FOREVER) == FALSE)
	{
		cout << "�ȴ������ܵ�ʵ��ʧ�ܣ�" << endl;
		return;
	}
 
	HANDLE hPipe = CreateFile(PIPE_NAME, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hPipe == INVALID_HANDLE_VALUE)
	{
		cout << "���������ܵ�ʧ�ܣ�" << endl;
		CloseHandle(hPipe);
		return;
	}
	cout << "����������ӳɹ���" << endl;
	while (1)
	{
		printf(buffer);//�ȴ���������
		if (WriteFile(hPipe, buffer, strlen(buffer), &WriteNum, NULL) == FALSE)
		{
			cout << "����д��ܵ�ʧ�ܣ�" << endl;
			break;
		}
	}
	
	cout << "�رչܵ���" << endl;
	CloseHandle(hPipe);
	system("pause");
}
