﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcmbExchangeRates
{
    public class Currency
    {
        public string Name { get; }
        public string Code { get; }
        public string CrossRateName { get; }
        public double ForexBuying { get; }
        public double ForexSelling { get; }
        public double BanknoteBuying { get; }
        public double BanknoteSelling { get; }

        public Currency(string name, string code, string crossRateName, double forexBuying, double forexSelling, double banknoteBuying, double banknoteSelling)
        {
            Name = name;
            Code = code;
            CrossRateName = crossRateName;
            ForexBuying = forexBuying;
            ForexSelling = forexSelling;
            BanknoteBuying = banknoteBuying;
            BanknoteSelling = banknoteSelling;

        }
    }
}
