// <copyright file="CookbookExamplesTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests;

[TestClass]
public class CookbookExamplesTests
{
    private static readonly Domain _domain8 = CreateTestDomain("test", 8);

    private static readonly Domain _domain10 = CreateTestDomain("test", 10);

    private static readonly Domain _domain12 = CreateTestDomain("test", 12);

    private static readonly IValueFactory _valueFactory8 = _domain8.ValueFactory;
    private static readonly IValueFactory _valueFactory10 = _domain10.ValueFactory;
    private static readonly IValueFactory _valueFactory12 = _domain12.ValueFactory;

    private static readonly IPseudonymFactory _pseudonymFactory8 = _domain8.PseudonymFactory;
    private static readonly IPseudonymFactory _pseudonymFactory10 = _domain10.PseudonymFactory;
    private static readonly IPseudonymFactory _pseudonymFactory12 = _domain12.PseudonymFactory;

    [TestMethod]
    public void Convert()
    {
        new string[][]
        {
            ["MQ==", "MQEAAAAAAAAAAA==", "AIaELoSge8ZH9YhlIeahODkJDe23j1NUsQqwm32j1o+CdG6lIKhi1KqdONLRsXh+ciZjddZtm2dShPpp3K5aDxD8"],
            ["MTI=", "MTICAAAAAAAAAAA=", "AURV0WACmddkLIQK2IF76J0XGOygU+mSZ5gnbQFQ7WSyEUq9H/c0738e2pwVTiSxcI1xz1dGqTcYfotir3K0LQrz"],
            ["MTIz", "MTIzAwAAAAAAAAAC", "QRJBFXWQueauTw1qeNzTM+3qZtLLbykEN8LCmrn29Nv3IXHlLFa9Hhd/iuRBHDsgv4usav38XCWgxroQ8zWruQ4="],
            ["MTIzNA==", "MTIzNAQAAAAAAAAAAg==", "CEI1kmrzzVyfeDxtQS3Gyme8//vbrYDneMIEmTFtaX2GyLhlA2JGNu0Wjd+qbpKUfMTB/rM+/H3jeAgFkR+lvIQ="],
            ["MTI0NQ==", "MTI0NQQAAAAAAAAAAA==", "AULi2K88MtySUCftPD3k2HAtq2myTVkgT33sMxm4Np2RJSdBJjPiamRyS3vIbKEaf0aNRan6B1lp4xJAr/ZAsdit"],
            ["MTIzNDU2", "MTIzNDU2BgAAAAAAAAAA", "AdnB30YG45HqxJyenfTwyomOV5NSCs9AbKKQFRBpEzDwuJ4FMak5UpxYElx7yGAGYjBcbEqV8vdjEBoGSewjYJx4"],
            ["MTIzNDU2Nw==", "MTIzNDU2NwcAAAAAAAAAAA==", "D5OUrNIZPGXYb+soXNh8VMiPYWudaCNAEZX/4B1EftfRMQOxStZBr23pCdCpaxpyZNc0HicDeBo+/TflO9kF10c="],
            ["MTIzNDU2Nzg=", "MTIzNDU2NzgIAAAAAAAAAAE=", "AfSlL6dUUnkvIowaMspc6avl4TvCqC4WE/NmEb1q3edqhmjBi8d3ku4GahorYpTkKDDGf1mV36ynC/o2/Zh8PqC7"],
            ["MTIzNDU2Nzg5", "MTIzNDU2Nzg5CQAAAAAAAAAB", "AWxzHDVoA6b08d7mpWsCn/AatpfglyUpsa4soLUzaS+HfRiwFT3EfvEd/1/CLVnVS0U/CUqT9tGNlhry0eWiyDMJ"],
            ["MTIzNDU2Nzg5MA==", "MTIzNDU2Nzg5MAoAAAAAAAAAAA==", "ALoZzeaX7m5Yz1YEUKfbyNYSOblCaUyK+CagReh+9BArAcI+d77cdV3iZQ52hI06Xfbd67J7jXbTKuVoRsDbCIdY"],
            ["RzDziSOxzz1fT6lMEPYT8C5xenPFTFwOhZe4CACeLbc=", "RzDziSOxzz1fT6lMEPYT8C5xenPFTFwOhZe4CACeLbcgAAAAAAAAAAA=", "ALARpzdxggw1mTjxYZKwdGOP0oyYKYjmqye1MewE9SP1zCp5wtSOpedAZNeyN1THUV0+WoXLUDCB1NZWT25xz5N6"],
            ["cmFuZG9tdGV4dA==", "cmFuZG9tdGV4dAoAAAAAAAAABA==", "AJXsoDpTMzMepP5g7Q0/aY11CrEp1cfgrw5+0S2XO5nNxzOu2jtpkgV9GOutVporRsKnLBs0VSglcRs+qINRK/4D"]
        }.ToList().ForEach(s =>
        {
            VerifyConvert(s, _valueFactory8);
        });

        new string[][]
        {
           ["MTIzNDU2Nzg5MA==", "MTIzNDU2Nzg5MAoAAAAAAAAAAAAC", "ANSl+G1Ihrn/y4KZmB2iO8ck7FXMvW6ExHVptUrKGfzInHfPukbx+xYNX9OhO/hRFQic3i7+Q6O6rlwhH4da4rrd"]
        }.ToList().ForEach(s =>
        {
            VerifyConvert(s, _valueFactory10);
        });

        new string[][]
        {
            ["MTIzNDU2Nzg5MA==", "MTIzNDU2Nzg5MAoAAAAAAAAAAAAAAAE=", "ASvP5vN+onuAc4jzCxECvkdBbUTfu3XpV6tUNGh3x68aUmMkz/6kFzrd+DEw/9g+oP1cLsJCuyYlzjq2PRVxwmrl"]
        }.ToList().ForEach(s =>
        {
            VerifyConvert(s, _valueFactory12);
        });
    }

    [TestMethod]
    public void Blind()
    {
        new string[][]
        {
            ["MQEAAAAAAAAAAA==",
            "AIaELoSge8ZH9YhlIeahODkJDe23j1NUsQqwm32j1o+CdG6lIKhi1KqdONLRsXh+ciZjddZtm2dShPpp3K5aDxD8",
            "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
            "AV+VXF9H5LdTe4b1SSC7bHjp6b2enJmfplC6a3/jCR5fUHxXRSaRniYR8h7ugNqalGvP49cZnv6lf9B72RUG0rA/",
            "eSmII52CEtsZzSseUDY3YKLtSgqhq1wLPm9ncHBzGiv1wMIxmc1jSmpW36GhTt/s1P5shZGhG8ncoWKSGkJDyfw="],
            ["MTICAAAAAAAAAAA=",
             "AURV0WACmddkLIQK2IF76J0XGOygU+mSZ5gnbQFQ7WSyEUq9H/c0738e2pwVTiSxcI1xz1dGqTcYfotir3K0LQrz",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "ANuVBi6VwQfa2F8Y+OM4NOQ3EtEOaLhWA92tYheMdK5DN9iubeC2pyy7gWEQKHkr5wx9VLv5ahX+3cqXdi+KvSn1",
             "ASbUF7UOw7WtsUKPk7yVCL9gPZSoP9DD2Mm0cxSUJd1qrO5A+mbQfQhKnQdkXp00T0U5kHNIhVKRNQtN7tIqW1FK"],
            ["MTIzAwAAAAAAAAAC",
             "QRJBFXWQueauTw1qeNzTM+3qZtLLbykEN8LCmrn29Nv3IXHlLFa9Hhd/iuRBHDsgv4usav38XCWgxroQ8zWruQ4=",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AJDTs17IHvcNMOdK7qZjfbnze1QFoTm8HBbvTpfD3khGbM01goo+fhkJ77VsCAZGSxny/JnlG6Vy0QE8hA/fX3JL",
             "APgdC1G8Whnf/F84mrlrw5FsJxy7GTkjjuVZmXFQTGvb9A2ppoyckftqX6Da8H2jib1wJUbspxPjfXXwrB6yMRgh"],
            ["MTIzNAQAAAAAAAAAAg==",
             "CEI1kmrzzVyfeDxtQS3Gyme8//vbrYDneMIEmTFtaX2GyLhlA2JGNu0Wjd+qbpKUfMTB/rM+/H3jeAgFkR+lvIQ=",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AXRwRG7lqfJT8IHCDFE0Zi6SNM0mciAf/q5SpS2gdAzZdAu+L+lFnahEtHKSLt86l+T45kl1CVP8OM7LdcFJYuJn",
             "AYUMbBVYgqgshWpf487AKX3ESaW2ftxzP90tW5hNOxPgETMzcsODv7oEU2Zdip3WZUa+ZhSL1avLJh68I3DjYGac"],
            ["MTI0NQQAAAAAAAAAAA==",
             "AULi2K88MtySUCftPD3k2HAtq2myTVkgT33sMxm4Np2RJSdBJjPiamRyS3vIbKEaf0aNRan6B1lp4xJAr/ZAsdit",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AKJ4Hl8nxryGEvH++kUsg908A1W5od0ZFFsx6M1ayvhHsVHEWaPah5zblwn5KhLVwcRmpYAH1CjLMexa2YEV3FCB",
             "AINtXU2NW833s/ZZng2LFykdRmKnEoJ/U+QUvtltalUaH/YNPK6fdIll7maOXwTY21Ch4BFwgdy+qbKGy/oQLGUc"],
            ["MTIzNDU2BgAAAAAAAAAA",
             "AdnB30YG45HqxJyenfTwyomOV5NSCs9AbKKQFRBpEzDwuJ4FMak5UpxYElx7yGAGYjBcbEqV8vdjEBoGSewjYJx4",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AVNW4mBCsSdzEs+JAi1vME8T4o5bB2chKvOiwr8ZxSu/53Epo8avyesca7XABzLqAWeFGHLQZxR9NZuE9O7Ztthf",
             "AUWBi3+hVu2oceV6Et67l1GieDF0WOehUD1ulk78TI0qdcE6IeJWx/XWaY052oOqHy+0WjBJXJ7Unvgf4oXiPVfd"],
            ["MTIzNDU2NwcAAAAAAAAAAA==",
             "D5OUrNIZPGXYb+soXNh8VMiPYWudaCNAEZX/4B1EftfRMQOxStZBr23pCdCpaxpyZNc0HicDeBo+/TflO9kF10c=",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AJjABYkAK6Oo5WkvSfVfubJSvH4px0T9Y5//P5gFv///BvAqF3AiGKwdUDLrbWRkEVjZOYB6icxImRWA5j1cJH0j",
             "AQiKLKsMCcpLZKXHMvgSjYWXKLIZA0+lyvbO+BzZiaxiiJUBlUdZG4PhN/3MwEHP85nMMxBVMg9DDSqQM2+KLqU1"],
            ["MTIzNDU2NzgIAAAAAAAAAAE=",
             "AfSlL6dUUnkvIowaMspc6avl4TvCqC4WE/NmEb1q3edqhmjBi8d3ku4GahorYpTkKDDGf1mV36ynC/o2/Zh8PqC7",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "AM95qyrZGpjcEpa/mXDyPdU/XkFQukQctmXRDUjS2Y1//x5BORsCxoRhEkOKYnt8u11g3vmm+dnc+t5Q37RevJzt",
             "AYZdFHNds2xdHG1ysp0YahcatO2mzyIjnN75NHo718Gxv2goRdvluyuJrCcH33Nnn/ei5NXrQNh9P8Q18uNReVgx"],
            ["MTIzNDU2Nzg5CQAAAAAAAAAB",
             "AWxzHDVoA6b08d7mpWsCn/AatpfglyUpsa4soLUzaS+HfRiwFT3EfvEd/1/CLVnVS0U/CUqT9tGNlhry0eWiyDMJ",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "Ad8a+82BH4JSpxQ/hT3jMEXLZ1AQ5zKmzyRviSrjfXZsCCpy2LXRbaBjpeAg1Yjl9xO5yazIfoqWxB/swoiKWYvc",
             "AJr1+M1tQeV+UH5Em1jNWCfmmOX+JVQNMt3myKJrY5o8b87gCnF/XqRkS/so6pCAo+s1ltrcDC6oGjpVxMp/GKR0"],
            ["MTIzNDU2Nzg5MAoAAAAAAAAAAA==",
             "ALoZzeaX7m5Yz1YEUKfbyNYSOblCaUyK+CagReh+9BArAcI+d77cdV3iZQ52hI06Xfbd67J7jXbTKuVoRsDbCIdY",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "ATXxK6NZFjvkwVVdDaZhxdloOo5FhvuZFse61cipoEk84ZLOk1w0qBqUsVWOm/upGAwMD8l1FmvuZ1gH+2u7ECd/",
             "AKGmtoy//h6dsi2nvNfywPA4g0G+vP94xkg+XU2Cd5vROjCbu109N/EtAf+3x+jcaUH00s6gO+ENinJ/I+hnJXs9"],
            ["MTIzNDU2Nzg5MAoAAAAAAAAAAAAC",
             "ANSl+G1Ihrn/y4KZmB2iO8ck7FXMvW6ExHVptUrKGfzInHfPukbx+xYNX9OhO/hRFQic3i7+Q6O6rlwhH4da4rrd",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "dQ1JgDTuuYJtHI0pQOiFJGDGBLoQYqyrENshTQKzHmDv5SSfsmbvN2PA553VleAM3xhxcB6mGmBOADrXsjhoRU8=",
             "AN++bpBMbAaib6MNKAUlfm/cZbKYWmG7xNyp5CqiIpR6MHATDMo6YGKvohBWncq/DUzet1DUKq1mpo1CCiGW6Mj6"],
            ["MTIzNDU2Nzg5MAoAAAAAAAAAAAAAAAE=",
             "ASvP5vN+onuAc4jzCxECvkdBbUTfu3XpV6tUNGh3x68aUmMkz/6kFzrd+DEw/9g+oP1cLsJCuyYlzjq2PRVxwmrl",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "fUzCiOY+2TINafVJV8Fw/b+FKmvupngaZa6P/GG25KRybVHIO8p2xHhW1xBsb1FvKskgAhIXUAoekeCRpPDTXuc=",
             "AdHBseAXBpudGLk+wmPOKAGv2af5y1KYqGsZfiNgGCZ6k+L91coa5HeBDOP9MIdT5dgdolvcbZXbUbYZUd3g/GmR"],
            ["RzDziSOxzz1fT6lMEPYT8C5xenPFTFwOhZe4CACeLbcgAAAAAAAAAAA=",
             "ALARpzdxggw1mTjxYZKwdGOP0oyYKYjmqye1MewE9SP1zCp5wtSOpedAZNeyN1THUV0+WoXLUDCB1NZWT25xz5N6",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "ARKh2805XnUxXwd7qAZ946L5HypbSD0J+A8W4Z+cmenFgeZFwbPkJpZ2tpso9EwOJkboGtVvzASFL/D/Lf94/vA=",
             "APNowE+iovhITW/UipIZQSFJGuTmYlacQDL+1bJgpbFtPrB3hv0jWrApaDOcYzgDFjTy1v892o18DnbO/z21SN1a"],
            ["cmFuZG9tdGV4dAoAAAAAAAAABA==",
             "AJXsoDpTMzMepP5g7Q0/aY11CrEp1cfgrw5+0S2XO5nNxzOu2jtpkgV9GOutVporRsKnLBs0VSglcRs+qINRK/4D",
             "AZ2TzlDmMWQW/kgJng4nThRUW149uzE+SNBUp4T0/0kRO2/zg2os/8dOEzZrvQPGgv5l0Gk68qczZjt8yfO8FRcF",
             "P/au8R8yR8aYFNOfY5EPu4DVlqXC+6Oh/Z5d6T/S4huWQgxsadxbcFRniWvnsjAsvitvRODmM8NK+nh1QwW1vIU=",
             "AeHg+C0RKMlsWzZem0sYd8+2+/i0XYHDCNiXTFz5fo6eHzOYFwUe3LLcPizLq/OhHWlJ5/zRlk5stjXpAiD/qpqT"]
        }.ToList().ForEach(strings =>
        {
            VerifyBlind(strings);
        });
    }

    [TestMethod]
    public void ComputeY()
    {
        new string[][]
        {
            ["MQEAAAAAAAAAAA==", "AIaELoSge8ZH9YhlIeahODkJDe23j1NUsQqwm32j1o+CdG6lIKhi1KqdONLRsXh+ciZjddZtm2dShPpp3K5aDxD8"],
            ["MTICAAAAAAAAAAA=", "AURV0WACmddkLIQK2IF76J0XGOygU+mSZ5gnbQFQ7WSyEUq9H/c0738e2pwVTiSxcI1xz1dGqTcYfotir3K0LQrz"],
            ["MTIzAwAAAAAAAAAC", "QRJBFXWQueauTw1qeNzTM+3qZtLLbykEN8LCmrn29Nv3IXHlLFa9Hhd/iuRBHDsgv4usav38XCWgxroQ8zWruQ4="],
            ["MTIzNAQAAAAAAAAAAg==", "CEI1kmrzzVyfeDxtQS3Gyme8//vbrYDneMIEmTFtaX2GyLhlA2JGNu0Wjd+qbpKUfMTB/rM+/H3jeAgFkR+lvIQ="],
            ["MTI0NQQAAAAAAAAAAA==", "AULi2K88MtySUCftPD3k2HAtq2myTVkgT33sMxm4Np2RJSdBJjPiamRyS3vIbKEaf0aNRan6B1lp4xJAr/ZAsdit"],
            ["MTIzNDU2BgAAAAAAAAAA", "AdnB30YG45HqxJyenfTwyomOV5NSCs9AbKKQFRBpEzDwuJ4FMak5UpxYElx7yGAGYjBcbEqV8vdjEBoGSewjYJx4"],
            ["MTIzNDU2NwcAAAAAAAAAAA==", "D5OUrNIZPGXYb+soXNh8VMiPYWudaCNAEZX/4B1EftfRMQOxStZBr23pCdCpaxpyZNc0HicDeBo+/TflO9kF10c="],
            ["MTIzNDU2NzgIAAAAAAAAAAE=", "AfSlL6dUUnkvIowaMspc6avl4TvCqC4WE/NmEb1q3edqhmjBi8d3ku4GahorYpTkKDDGf1mV36ynC/o2/Zh8PqC7"],
            ["MTIzNDU2Nzg5CQAAAAAAAAAB", "AWxzHDVoA6b08d7mpWsCn/AatpfglyUpsa4soLUzaS+HfRiwFT3EfvEd/1/CLVnVS0U/CUqT9tGNlhry0eWiyDMJ"],
            ["MTIzNDU2Nzg5MAoAAAAAAAAAAA==", "ALoZzeaX7m5Yz1YEUKfbyNYSOblCaUyK+CagReh+9BArAcI+d77cdV3iZQ52hI06Xfbd67J7jXbTKuVoRsDbCIdY"],
            ["RzDziSOxzz1fT6lMEPYT8C5xenPFTFwOhZe4CACeLbcgAAAAAAAAAAA=", "ALARpzdxggw1mTjxYZKwdGOP0oyYKYjmqye1MewE9SP1zCp5wtSOpedAZNeyN1THUV0+WoXLUDCB1NZWT25xz5N6"],
            ["cmFuZG9tdGV4dAoAAAAAAAAABA==", "AJXsoDpTMzMepP5g7Q0/aY11CrEp1cfgrw5+0S2XO5nNxzOu2jtpkgV9GOutVporRsKnLBs0VSglcRs+qINRK/4D"]
        }.ToList().ForEach(strings =>
        {
            VerifyComputeY(strings, _pseudonymFactory8);
        });

        new string[][]
        {
           ["MTIzNDU2Nzg5MAoAAAAAAAAAAAAC", "ANSl+G1Ihrn/y4KZmB2iO8ck7FXMvW6ExHVptUrKGfzInHfPukbx+xYNX9OhO/hRFQic3i7+Q6O6rlwhH4da4rrd"]
        }.ToList().ForEach(strings =>
        {
            VerifyComputeY(strings, _pseudonymFactory10);
        });

        new string[][]
        {
            ["MTIzNDU2Nzg5MAoAAAAAAAAAAAAAAAE=", "ASvP5vN+onuAc4jzCxECvkdBbUTfu3XpV6tUNGh3x68aUmMkz/6kFzrd+DEw/9g+oP1cLsJCuyYlzjq2PRVxwmrl"]
        }.ToList().ForEach(strings =>
        {
            VerifyComputeY(strings, _pseudonymFactory12);
        });
    }

    private void VerifyComputeY(string[] strings, IPseudonymFactory pseudonymFactory)
    {
        if (strings != null)
        {
            var pseudonym = pseudonymFactory.FromX(strings[0]);
            Assert.AreEqual(ToLongString(strings[0]), pseudonym.X());

            var actualY = pseudonym.Y();
            var expectedY = ToLongString(strings[1]);
            var expectedYInverted = ((Pseudonym)pseudonymFactory.FromXy(strings[0], strings[1])).Multiply(new BigInteger("-1")).Y();

            Assert.IsTrue(actualY == expectedY || actualY == expectedYInverted);
        }
    }

    private void VerifyBlind(string[] strings)
    {
        var random = new BigInteger(System.Convert.FromBase64String(strings[2]));
        var pseudonym = _domain8.PseudonymFactory.FromXy(strings[0], strings[1]);
        var blindedValue = pseudonym.Multiply(random);
        Assert.AreEqual(ToLongString(strings[3]), blindedValue.X());
        Assert.AreEqual(ToLongString(strings[4]), blindedValue.Y());
    }

    private void VerifyConvert(string[] strings, IValueFactory valueFactory)
    {
        if (strings != null)
        {
            var value = valueFactory!.From(System.Convert.FromBase64String(strings![0]));
            Assert.AreEqual(ToLongString(strings[1]), value.X());
            Assert.AreEqual(ToLongString(strings[2]), value.Y());
        }
    }

    private string ToLongString(string shortString)
    {
        var decoded = System.Convert.FromBase64String(shortString);
        var logBytes = new byte[66];
        Array.Copy(decoded, 0, logBytes, 66 - decoded.Length, decoded.Length);
        return System.Convert.ToBase64String(logBytes);
    }
}
