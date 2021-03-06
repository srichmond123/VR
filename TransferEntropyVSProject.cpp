// TransferEntropyVSProject.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <fstream>
#include <iostream>
#include <string>
#include <vector>
#include <map>
#include <algorithm>
#include <math.h>
#include <tuple>

using namespace std;

//void downsample();
double TransferEntropy(vector<float>&, vector<float>&);

struct Symbol {
	Symbol(float x, float y, float z) {
		if (x < y && x < z)
			this->x = 0;
		else if (x < y || x < z)
			this->x = 1;
		else
			this->x = 2;
		if (y < z) {
			if (this->x == 0) {
				this->y = 1;
				this->z = 2;
			}
			else {
				this->y = 0;
				if (z > x)
					this->z = 2;
				else
					this->z = 1;
			}
		}
		else {
			if (this->x == 0) {
				this->y = 2;
				this->z = 1;
			}
			else {
				this->y = 1;
				if (z > x)
					this->z = 1;
				else
					this->z = 0;
			}
		}
	}
	
	bool operator==(const Symbol& other) const {
		return this->x == other.x && this->y == other.y && this->z == other.z;
	}
	bool operator!=(const Symbol& other) const {
		return *this != other;
	}

	bool operator<(const Symbol& other) const {
		if (this->x != other.x)
			return this->x < other.x;
		else if (this->y != other.y)
			return this->y < other.y;
		else
			return this->z < other.z;
	}

	int x;
	int y;
	int z;
};

ostream& operator<<(ostream& os, const Symbol& s) {
	os << "(" << s.x << ", " << s.y << ", " << s.z << ")";
	return os;
}

int symbol_length = 3;

int main()
{
	vector<float> X;
	vector<float> Y;

	ifstream ifs("Book1.csv");
	string word;
	if (ifs) {
		while (ifs >> word) {
			if (word.find('X') == -1) {
				X.push_back(stof(word.substr(0, word.find(','))));
				Y.push_back(stof(word.substr(word.find(',') + 1)));
			}
		}
	}
	ifs.close();

	/*
	for (int i : X) {
		cout << i << " ";
	}
	cout << "----";
	for (int i : Y) {
		cout << i << " ";
	}
	cout << "----";

	system("pause");
	*/
	double teXY = TransferEntropy(X, Y);
	double teYX = TransferEntropy(Y, X);

	cout << "Entropy X->Y: " << teXY << endl;
	cout << "Entropy Y->X: " << teYX << endl;
	system("pause");

    return 0;
}


void downsample(ifstream& ifs) {
	////
}

void getSymbols(vector<Symbol>& sym, vector<float>& vec) {
	for (size_t i = 0; i < vec.size() - symbol_length + 1; i++) { //Symbol is length 3
		Symbol s(vec[i], vec[i + 1], vec[i + 2]);
		sym.push_back(s);
	}
}

double ShannonEntropyTriple(vector<Symbol>& a, vector<Symbol>& b, vector<Symbol>& c) {
	map<tuple<Symbol, Symbol, Symbol>, int> mp;

	for (size_t i = 0; i < a.size(); i++) {
		mp[tuple < Symbol, Symbol, Symbol>{a[i], b[i], c[i]}]++;
	}

	map<tuple<Symbol, Symbol, Symbol>, int>::iterator it;
	size_t count = a.size();
	double entropy = 0;

	for (it = mp.begin(); it != mp.end(); it++) {
		entropy += (-it->second / (double)count) * log(it->second / (double)count);
	}

	return entropy;
}

double ShannonEntropyPair(vector<Symbol>& a, vector<Symbol>& b) {
	map<pair<Symbol, Symbol>, int> mp;

	for (size_t i = 0; i < a.size(); i++) {
		mp[ pair<Symbol, Symbol>{a[i], b[i]} ]++;
	}
	
	map<pair<Symbol, Symbol>, int>::iterator it;
	size_t count = a.size();
	double entropy = 0;

	for (it = mp.begin(); it != mp.end(); it++)
	{
		entropy += (-it->second / (double)count) * log(it->second / (double)count);
	}

	return entropy;
}

double ShannonEntropy(vector<Symbol>& vec) {
	map<Symbol, int> mp;

	for (size_t i = 0; i < vec.size(); i++) {
		mp[vec[i]]++;
	}

	//Calculate entropy (sum of -p(x)*ln(p(x)) )
	size_t count = vec.size();

	map<Symbol, int>::iterator it;
	double entropy = 0;

	for (it = mp.begin(); it != mp.end(); it++)
	{
		entropy += (-it->second / (double)count) * log(it->second / (double)count);
	}
	return entropy;
}

double TransferEntropy(vector<float>& X, vector<float>& Y) {
	vector<Symbol> symbolsX_t;
	getSymbols(symbolsX_t, X);
	symbolsX_t.erase(symbolsX_t.begin());

	vector<Symbol> symbolsX_t_prev;
	getSymbols(symbolsX_t_prev, X);
	symbolsX_t_prev.pop_back();

	vector<Symbol> symbolsY_t;
	getSymbols(symbolsY_t, Y);
	symbolsY_t.erase(symbolsY_t.begin());

	vector<Symbol> symbolsY_t_prev;
	getSymbols(symbolsY_t_prev, Y);
	symbolsY_t_prev.pop_back();

	for (Symbol i : symbolsX_t) {
		cout << i << " ";
	}
	cout << endl;
	for (Symbol i : symbolsX_t_prev) {
		cout << i << " ";
	}
	cout << endl;

	double transfer_entropy = 0; //Transfer entropy from data in column X to column Y:
	transfer_entropy += ShannonEntropyPair(symbolsY_t, symbolsY_t_prev) - ShannonEntropy(symbolsY_t_prev);
	transfer_entropy += -ShannonEntropyTriple(symbolsY_t, symbolsY_t_prev, symbolsX_t_prev);
	transfer_entropy += ShannonEntropyPair(symbolsY_t_prev, symbolsX_t_prev);

	return transfer_entropy;
}
