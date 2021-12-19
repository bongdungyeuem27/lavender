import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import * as articleApi from "../apis/article";
import LoadingContainer from "../../Common/helper/loading/LoadingContainer";
import { stubTrue } from "lodash";

function Article() {
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(stubTrue);
  useEffect(() => {
    (async () => {
      await articleApi
        .allArticle()
        .then((res) => {
          console.log(res);
          setPosts(res.data);
        })
        .catch((err) => {
          console.log(err);
        });
      setLoading(false);
    })();
  }, []);
  return (
    <div>
      <LoadingContainer loading={loading}></LoadingContainer>
      <section>
        <div id="wrapper">
          <div className="container">
            <div className="row">
              <div className="col-9">
                <div className="page-wrapper">
                  <div className="blog-top clearfix mb-5">
                    <h3 className="pull-left">Tin công nghệ</h3>
                  </div>
                  {posts.map((post) => (
                    <div className="blog-box row mb-4">
                      <div className="col-md-4">
                        <div className="post-media">
                          <Link to={`/article/${post.mabaiviet}`} className="">
                            <img
                              src={post.thumnail}
                              alt=""
                              className="img-fluid"
                            />
                            <div className="hovereffect" />
                          </Link>
                        </div>
                      </div>
                      <div className="blog-meta big-meta col-8">
                        <h5>
                          <Link
                            to={`/articledetail/${post.mabaiviet}`}
                            className="post-title"
                          >
                            {post.tieude}
                          </Link>
                        </h5>
                        {/* <h5><a type="button"
                                                        className=""
                                                        onClick={toDetail(post.mabaiviet)}
                                                    >
                                                        {post.tieude}
                                                    </a></h5> */}
                        <p className="post-description">{post.mota}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

export default Article;
